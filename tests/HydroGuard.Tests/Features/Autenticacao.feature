# language: pt
Funcionalidade: Autenticação

  Cenário: Rejeitar credenciais inválidas
    Dado que existe um usuário de teste
    Quando tento entrar com uma senha incorreta
    Então a API rejeita as credenciais sem emitir token
