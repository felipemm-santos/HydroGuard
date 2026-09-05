// ===== HydroGuard - Frontend JavaScript =====
// Aplicação frontend que consome a API REST com autenticação JWT

// URL base da API (mesma origem quando servido pelo ASP.NET)
const API_BASE = '';

// Token JWT salvo no localStorage após login
let authToken = localStorage.getItem('hydroguard_token');

// ===== INICIALIZAÇÃO =====
// Quando a página carrega, verifica se já está logado
document.addEventListener('DOMContentLoaded', () => {
  if (authToken) {
  // Se tem token salvo, mostra o app
    showApp();
  } else {
  // Se não tem token, mostra login
    showLogin();
  }

  // Registra eventos dos formulários e botões
  setupEventListeners();
});

// ===== AUTENTICAÇÃO =====

/**
 * Faz login na API e salva o token JWT.
 * POST /api/auth/login
 */
async function login(username, password) {
  const response = await fetch(`${API_BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });

  if (!response.ok) {
    throw new Error('Credenciais inválidas');
  }

  const data = await response.json();
  // Salva token no localStorage para persistir sessão
  authToken = data.token;
  localStorage.setItem('hydroguard_token', authToken);
  return data;
}

/**
 * Faz logout - remove token e volta ao login.
 */
function logout() {
  authToken = null;
  localStorage.removeItem('hydroguard_token');
  showLogin();
}

/**
 * Requisição genérica à API com token JWT no header.
 */
async function apiRequest(url, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...(authToken && { Authorization: `Bearer ${authToken}` })
  };

  const response = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers: { ...headers, ...options.headers }
  });

  if (response.status === 401) {
    logout();
    throw new Error('Sessão expirada. Faça login novamente.');
  }

  return response;
}

// ===== NAVEGAÇÃO DE TELAS =====

function showLogin() {
  document.getElementById('login-section').classList.add('visible');
  document.getElementById('app-content').classList.remove('visible');
  document.getElementById('navbar').style.display = 'none';
}

function showApp() {
  document.getElementById('login-section').classList.remove('visible');
  document.getElementById('app-content').classList.add('visible');
  document.getElementById('navbar').style.display = 'flex';
  // Carrega dados iniciais do dashboard
  loadDashboard();
  loadMedidores();
}

function showSection(sectionId) {
  // Esconde todas as seções
  document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));
  document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));

  // Mostra a seção selecionada
  document.getElementById(`section-${sectionId}`).classList.add('active');
  document.querySelector(`[data-section="${sectionId}"]`).classList.add('active');

  // Carrega dados da seção
  if (sectionId === 'dashboard') loadDashboard();
  if (sectionId === 'medidores') loadMedidores();
  if (sectionId === 'consumos') { loadMedidoresSelect(); loadConsumos(); }
  if (sectionId === 'alertas') { /* aguarda clique do botão */ }
}

// ===== EVENT LISTENERS =====

function setupEventListeners() {
  // Formulário de login
  document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById('login-message');
    try {
      await login(
        document.getElementById('username').value,
        document.getElementById('password').value
      );
      msgEl.innerHTML = '';
      showApp();
    } catch (err) {
      msgEl.innerHTML = `<div class="message error">${err.message}</div>`;
    }
  });

  // Logout
  document.getElementById('btn-logout').addEventListener('click', logout);

  // Navegação
  document.querySelectorAll('.nav-link[data-section]').forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      showSection(link.dataset.section);
    });
  });

  // Cadastro de medidor
  document.getElementById('medidor-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById('medidor-message');
    try {
      const response = await apiRequest('/api/medidores', {
        method: 'POST',
        body: JSON.stringify({
          nome: document.getElementById('medidor-nome').value,
          serial: document.getElementById('medidor-serial').value,
          localizacao: document.getElementById('medidor-localizacao').value
        })
      });

      if (response.ok) {
        msgEl.innerHTML = '<div class="message success">Medidor cadastrado com sucesso!</div>';
        document.getElementById('medidor-form').reset();
        loadMedidores();
      } else {
        const err = await response.json();
        msgEl.innerHTML = `<div class="message error">${err.message || 'Erro ao cadastrar'}</div>`;
      }
    } catch (err) {
      msgEl.innerHTML = `<div class="message error">${err.message}</div>`;
    }
  });

  // Registro de consumo
  document.getElementById('consumo-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const msgEl = document.getElementById('consumo-message');
    try {
      const response = await apiRequest('/api/consumos', {
        method: 'POST',
        body: JSON.stringify({
          medidorId: parseInt(document.getElementById('consumo-medidor').value),
          litrosConsumidos: parseFloat(document.getElementById('consumo-litros').value)
        })
      });

      if (response.ok) {
        msgEl.innerHTML = '<div class="message success">Consumo registrado com sucesso!</div>';
        document.getElementById('consumo-form').reset();
        loadConsumos();
      } else {
        const err = await response.json();
        msgEl.innerHTML = `<div class="message error">${err.message || 'Erro ao registrar'}</div>`;
      }
    } catch (err) {
      msgEl.innerHTML = `<div class="message error">${err.message}</div>`;
    }
  });

  // Detectar vazamentos
  document.getElementById('btn-detectar').addEventListener('click', loadAlertas);
}

// ===== CARREGAR DADOS DA API =====

/**
 * Carrega indicadores ESG e relatório no dashboard.
 */
async function loadDashboard() {
  try {
    // Busca indicadores ESG
    const esgRes = await apiRequest('/api/esg/indicadores');
    const esg = await esgRes.json();

    // Monta cards de indicadores
    document.getElementById('dashboard-indicators').innerHTML = `
      <div class="indicator-card">
        <div class="value">${esg.consumoMensal.toLocaleString()} L</div>
        <div class="label">Consumo Mensal</div>
      </div>
      <div class="indicator-card">
        <div class="value">${esg.consumoMedioDiario.toLocaleString()} L</div>
        <div class="label">Média Diária</div>
      </div>
      <div class="indicator-card">
        <div class="value">${esg.desperdicioEstimado.toLocaleString()} L</div>
        <div class="label">Desperdício Estimado</div>
      </div>
      <div class="indicator-card">
        <div class="value">${esg.aguaEconomizada.toLocaleString()} L</div>
        <div class="label">Água Economizada</div>
      </div>
      <div class="indicator-card score">
        <div class="value">${esg.scoreESG}</div>
        <div class="label">Score ESG</div>
      </div>
    `;

    // Busca relatório gerencial
    const relRes = await apiRequest('/api/relatorios/consumo');
    const rel = await relRes.json();

    let topHtml = '<h3 style="margin-bottom:0.5rem;">Top Medidores</h3><table class="table"><thead><tr><th>Medidor</th><th>Consumo (L)</th></tr></thead><tbody>';
    rel.topMedidores.forEach(m => {
      topHtml += `<tr><td>${m.nome}</td><td>${m.totalConsumo.toLocaleString()}</td></tr>`;
    });
    topHtml += '</tbody></table>';

    document.getElementById('relatorio-content').innerHTML = `
      ${topHtml}
      <p style="margin-top:1rem;"><strong>Média por Período:</strong> ${rel.mediaPorPeriodo.toLocaleString()} L</p>
      <p><strong>Projeção Futura:</strong> ${rel.projecaoFutura.toLocaleString()} L</p>
      <p><strong>Alertas Críticos:</strong> ${rel.alertasCriticos.length}</p>
    `;
  } catch (err) {
    document.getElementById('dashboard-indicators').innerHTML = `<div class="message error">${err.message}</div>`;
  }
}

/**
 * Carrega lista de medidores paginada.
 */
async function loadMedidores() {
  try {
    const response = await apiRequest('/api/medidores?page=1&pageSize=20');
    const data = await response.json();

    if (data.items.length === 0) {
      document.getElementById('medidores-list').innerHTML = '<p>Nenhum medidor cadastrado.</p>';
      return;
    }

    let html = '<table class="table"><thead><tr><th>ID</th><th>Nome</th><th>Serial</th><th>Localização</th></tr></thead><tbody>';
    data.items.forEach(m => {
      html += `<tr><td>${m.id}</td><td>${m.nome}</td><td>${m.serial}</td><td>${m.localizacao}</td></tr>`;
    });
    html += '</tbody></table>';
    html += `<p style="margin-top:0.5rem;color:var(--text-light);">Total: ${data.totalRecords} medidores</p>`;

    document.getElementById('medidores-list').innerHTML = html;
  } catch (err) {
    document.getElementById('medidores-list').innerHTML = `<div class="message error">${err.message}</div>`;
  }
}

/**
 * Popula select de medidores no formulário de consumo.
 */
async function loadMedidoresSelect() {
  try {
    const response = await apiRequest('/api/medidores?page=1&pageSize=100');
    const data = await response.json();
    const select = document.getElementById('consumo-medidor');
    select.innerHTML = '<option value="">Selecione um medidor</option>';
    data.items.forEach(m => {
      select.innerHTML += `<option value="${m.id}">${m.nome} (${m.serial})</option>`;
    });
  } catch (err) {
    console.error('Erro ao carregar medidores:', err);
  }
}

/**
 * Carrega histórico de consumos.
 */
async function loadConsumos() {
  try {
    const response = await apiRequest('/api/consumos?page=1&pageSize=20');
    const data = await response.json();

    if (data.items.length === 0) {
      document.getElementById('consumos-list').innerHTML = '<p>Nenhum consumo registrado.</p>';
      return;
    }

    let html = '<table class="table"><thead><tr><th>ID</th><th>Medidor</th><th>Litros</th><th>Data</th></tr></thead><tbody>';
    data.items.forEach(c => {
      const dataFmt = new Date(c.dataRegistro).toLocaleString('pt-BR');
      html += `<tr><td>${c.id}</td><td>${c.medidorId}</td><td>${c.litrosConsumidos}</td><td>${dataFmt}</td></tr>`;
    });
    html += '</tbody></table>';
    html += `<p style="margin-top:0.5rem;color:var(--text-light);">Total: ${data.totalRecords} registros</p>`;

    document.getElementById('consumos-list').innerHTML = html;
  } catch (err) {
    document.getElementById('consumos-list').innerHTML = `<div class="message error">${err.message}</div>`;
  }
}

/**
 * Detecta e exibe alertas de vazamento.
 */
async function loadAlertas() {
  const listEl = document.getElementById('alertas-list');
  listEl.innerHTML = 'Analisando consumos...';

  try {
    const response = await apiRequest('/api/alertas/vazamentos');
    const alertas = await response.json();

    if (alertas.length === 0) {
      listEl.innerHTML = '<div class="message success">Nenhum vazamento detectado. Tudo normal! ✅</div>';
      return;
    }

    let html = '';
    alertas.forEach(a => {
      const classe = a.severidade.toLowerCase();
      html += `
        <div class="alert-item ${classe}">
          <strong>Medidor #${a.medidorId}</strong> - Severidade: ${a.severidade}
          <br>${a.mensagem}
        </div>
      `;
    });

    listEl.innerHTML = html;
  } catch (err) {
    listEl.innerHTML = `<div class="message error">${err.message}</div>`;
  }
}
