# Deploy do HydroGuard em VM Azure por SSH

## Resultado e arquitetura

Build, testes, build Docker e publicação continuam em runners GitHub Ubuntu. Os jobs de deploy enviam um pacote do commit por SSH à VM, baixam a mesma imagem do GHCR por digest e executam o smoke HTTP em C#. Staging deve passar antes de produção. Não é necessário instalar um self-hosted runner nem contratar Azure Container Registry.

Os serviços permanecem na VM após o job. Os bancos são independentes, com volumes persistentes nomeados pelos projetos `hydroguard-staging` e `hydroguard-production`. A API de staging escuta apenas em `127.0.0.1:5001`; produção, em `127.0.0.1:5000`. O banco não publica porta no host. A VM deve permanecer ligada durante deploy e demonstração.

## 1. Preparar a VM existente

Configuração de referência informada: `vm-hydroguard`, grupo `rg-hydroguard`, região South Africa North, Ubuntu Server 24.04 LTS x64, Standard B2as v2 (2 vCPUs, 8 GiB), usuário `azureuser`.

Docker e Compose já foram validados com `hello-world`. Na VM, confirme as outras ferramentas:

```bash
sudo apt update
sudo apt install -y git dotnet-sdk-8.0
mkdir -p "$HOME/hydroguard"
docker version
docker compose version
dotnet --list-sdks
command -v tar flock
```

O usuário SSH precisa executar Docker sem sudo. O SDK .NET 8 é usado somente para compilar/executar o smoke; API e SQL Server rodam em containers. Todos os testes da aplicação permanecem em C#. Python não é necessário para o pipeline nem para o deploy.

Não precisa clonar o repositório na VM. O workflow envia Compose, scripts e smoke do commit correspondente. Cada execução fica em `~/hydroguard/<ambiente>/releases/<commit>-<run_id>-<tentativa>/`. Esses diretórios contêm os arquivos de deploy e smoke; não são os volumes do banco. Versões antigas podem acumular arquivos de build do smoke; acompanhe o espaço livre com `df -h`.

## 2. Configurar chave SSH de automação

Pode utilizar a chave `.pem` já autorizada da VM. Para separar a automação do seu acesso pessoal, é preferível criar uma chave dedicada.

**No Git Bash do Windows:**

```bash
ssh-keygen -t ed25519 -C "hydroguard-github-actions" -f ~/.ssh/hydroguard-actions
```

Para execução não interativa deste workflow, deixe a passphrase vazia. A implementação não abre prompts nem usa senha para desbloquear a chave. O arquivo sem `.pub` é privado e será cadastrado como secret; nunca o versione.

Exiba a chave pública no Windows:

```bash
cat ~/.ssh/hydroguard-actions.pub
```

**Na sessão SSH da VM**, acrescente essa linha ao `authorized_keys`, sem substituir as chaves existentes:

```bash
mkdir -p ~/.ssh
chmod 700 ~/.ssh
nano ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
```

No nano: cole a chave pública em uma linha nova, salve com Ctrl+O, Enter e saia com Ctrl+X. Teste no Windows, substituindo `IP_PUBLICO`:

```bash
ssh -i ~/.ssh/hydroguard-actions azureuser@IP_PUBLICO
```

## 3. Fixar a identidade do servidor (known_hosts)

`SSH_PRIVATE_KEY` autentica o usuário. `SSH_KNOWN_HOSTS` identifica a VM. Não são a mesma chave.

Em uma sessão confiável da VM (ou usando Run Command do portal para obter a informação pelo canal Azure), obtenha sua chave pública de servidor:

```bash
cat /etc/ssh/ssh_host_ed25519_key.pub
ssh-keygen -lf /etc/ssh/ssh_host_ed25519_key.pub
```

A chave pública pode ser compartilhada; a chave privada do servidor nunca deve sair da VM. Monte o valor do secret com **o mesmo IP público ou DNS usado em SSH_HOST**, um espaço e os dois primeiros campos da chave pública:

```text
IP_PUBLICO ssh-ed25519 BASE64_DA_CHAVE_PUBLICA_DO_SERVIDOR
```

Substitua os marcadores pelos valores reais, em uma única linha. O comentário final é opcional. Não use o IP privado `172.16.0.4` para acessar a VM a partir do GitHub. A implementação usa a porta SSH padrão 22 e exige `StrictHostKeyChecking=yes`. Se recriar a VM ou alterar o endereço, confira e atualize os secrets correspondentes.

## 4. Configurar GitHub

Em **Settings > Actions > General**, habilite Actions e permita as actions utilizadas no YAML. Os jobs declaram as permissões necessárias: escrita de packages somente para publicação e leitura para deploy. O token automático `GITHUB_TOKEN` é usado no GHCR; não é necessário PAT. Se o pacote já existir, confira **Package settings > Manage Actions access** para este repositório.

Em **Settings > Environments**, crie `staging` e `production`. Em cada um, selecione **Deployment branches and tags > Selected branches and tags** e permita a branch `main`. Para promoção automática, não configure revisores obrigatórios nem tempo de espera.

Cadastre os seguintes **Environment secrets** em ambos:

| Secret | Valor |
| --- | --- |
| `SSH_HOST` | IP público ou DNS da VM, sem `http://`, usuário ou porta |
| `SSH_USER` | `azureuser` |
| `SSH_PRIVATE_KEY` | Conteúdo completo da chave privada dedicada (ou `.pem` já autorizada), incluindo BEGIN/END e quebras de linha; sem passphrase |
| `SSH_KNOWN_HOSTS` | Linha montada no passo 3 com a chave pública do servidor |
| `MSSQL_SA_PASSWORD` | Senha do SQL Server desse ambiente |
| `JWT_KEY` | Chave JWT desse ambiente |
| `ADMIN_PASSWORD` | Senha inicial do admin desse ambiente |

As informações SSH podem ser iguais para os dois environments, pois a VM é a mesma. Use credenciais da aplicação e banco diferentes em staging e produção. Use senhas geradas localmente, com pelo menos 12 caracteres; SQL exige maiúscula, minúscula, número e símbolo. Os validadores aceitam letras, números e `!@#%_+=.-`, sem espaços, aspas, `$` ou `;`. JWT deve ter pelo menos 32 bytes (por exemplo, 64 caracteres hexadecimais aleatórios).

| Environment variable opcional | staging | production |
| --- | --- | --- |
| `API_PORT` | `5001` (padrão) | `5000` (padrão) |

Mantenha as portas diferentes. `BIND_ADDRESS` fica fixo em `127.0.0.1` no transporte remoto. `APP_URL` não é utilizada; `localhost` não representa uma URL pública de deployment.

Credenciais são transmitidas dentro de um pacote privado temporário, com permissões restritas e transporte SSH. O arquivo `.deploy.env` é removido antes do deploy e novamente na finalização; o Docker usa um diretório de autenticação temporário. Não publique esse pacote nem a configuração resolvida. Como em qualquer deploy Docker por variáveis, o administrador da VM pode consultar as credenciais dos containers. Em caso de desligamento abrupto, confira e remova arquivos `.deploy.env` remanescentes nos diretórios de release antes de compartilhar a VM.

Se já existir banco no volume, use as senhas correspondentes. Alterar os secrets não redefine automaticamente as senhas já criadas no SQL Server ou do usuário admin.

## 5. Permitir o acesso pela rede

Na Azure, confira **VM > Networking / Network settings** e o NSG associado: a porta TCP 22 deve aceitar o executor do deploy. Uma regra limitada apenas ao IP da sua casa permite seu terminal, mas bloqueia os runners hospedados do GitHub.

Este workflow usa SSH direto e não altera o NSG automaticamente. A regra de origem precisa abranger os endereços dos runners utilizados; os runners padrão têm IPs variáveis. Se sua política de rede exigir uma origem fixa, será preciso um executor com IP fixo ou outro transporte (por exemplo, Azure Run Command com OIDC). Não é necessário abrir 5000, 5001 ou 1433 para demonstrar por túnel.

A VM precisa de saída HTTPS para GitHub Container Registry, Microsoft Container Registry e NuGet. SSH com timeout costuma indicar VM desligada, IP incorreto, rota ou regra de rede bloqueando.

## 6. Executar o pipeline

1. Aplique os arquivos atualizados no repositório e faça commit.
2. Abra PR para `main`: ele executa build/testes e validação dos containers, sem SSH, publicação ou deploy.
3. Após merge, acompanhe os quatro jobs: `build-test`, `container`, `deploy-staging`, `deploy-production`.
4. Alternativamente, use **Actions > HydroGuard CI-CD > Run workflow**, selecionando `main`, depois que o workflow estiver nessa branch.

Push somente em branch de trabalho não dispara o YAML atual. Deploy só ocorre em `main`, fora de pull requests. O envio contém apenas arquivos necessários e usa o digest da imagem publicada pelo job `container`; staging e produção não recompilam a imagem. Um lock na VM serializa os deploys. Não há limpeza de containers/volumes nesses dois ambientes; somente a stack `hydroguard-ci` do runner é descartada.

## 7. Demonstrar a interface pelo Windows

No Git Bash local, usando sua chave pessoal e o IP real:

```bash
ssh -i vm-hydroguard_key.pem -N \
  -L 5001:127.0.0.1:5001 \
  -L 5000:127.0.0.1:5000 \
  azureuser@IP_PUBLICO
```

Deixe esse terminal aberto. Abra:

- Staging: http://localhost:5001
- Produção: http://localhost:5000

Entre com `admin` e a senha do environment correspondente. Os bancos começam somente com o administrador; cadastre dados pela interface para os prints. Se as portas já estiverem ocupadas no Windows, troque apenas as portas à esquerda do `-L` (ex.: `15001:127.0.0.1:5001`) e acesse `localhost:15001`.

## 8. Comprovação por prints e persistência

O pipeline não gera nem publica pacotes de evidências. Os testes xUnit e o smoke mostram seus resultados diretamente no log do Actions. O smoke retorna erro se uma verificação falhar; staging precisa ser aprovado antes da promoção para produção.

Capture os seguintes prints para a entrega:

1. Resumo do Actions com os quatro jobs aprovados.
2. Resultado dos testes C#, mostrando aprovados e falhas.
3. Imagem publicada no GHCR.
4. Logs dos dois deploys, mostrando ambiente, commit e o mesmo digest.
5. Containers saudáveis na VM.
6. Telas da aplicação em staging e produção pelo túnel SSH.

Na VM, confira os containers e volumes:

```bash
docker ps --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
docker volume ls --filter name=hydroguard
```

O resultado de cada smoke é impresso como JSON no terminal, sem gravar arquivo. O resumo do job também registra a imagem, o commit e o sucesso do deploy. Não são gerados TRX, cobertura ou arquivos de diagnóstico pelo workflow atual.

Para comprovar isolamento e persistência, cadastre dados distintos em staging/produção, execute novo deploy e confirme os registros nos respectivos ambientes. Os volumes ficam em disco gerenciado da VM. Não remova volumes ou o disco se quiser manter os dados; não há backup automático neste projeto.

## 9. Falhas e operação

- `Permission denied (publickey)`: usuário/chave incorretos, chave com passphrase ou pública não autorizada.
- `Host key verification failed`: compare a chave real do servidor e o endereço com `SSH_KNOWN_HOSTS`; não desative a verificação.
- `docker: permission denied`: confirme o grupo Docker na sessão do usuário SSH; reconecte após `usermod`.
- `dotnet: command not found`: instale SDK .NET 8 na VM.
- `denied` no pull GHCR: confira o acesso do repositório ao pacote.
- SQL ou API unhealthy: consulte o log do job e os logs dos containers na VM; revise secrets e recursos da VM.
- Staging falhou: produção não é promovida. Os recursos já iniciados ficam disponíveis para diagnóstico.

Não há rollback automático. Reverter o código por commit e executar o fluxo publica/promove a versão revertida, desde que ela seja compatível com as migrations já aplicadas. Reverter a imagem não reverte o banco.

A VM pode hospedar ambos os ambientes para fins acadêmicos. Isso não fornece alta disponibilidade. Ao terminar, desaloque a VM pelo portal para interromper a cobrança de processamento; discos/IP podem continuar gerando custo. Ao excluir definitivamente a infraestrutura, confira os recursos remanescentes antes de encerrar a assinatura.

## Referências

- [Secrets no GitHub Actions](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets)
- [SSH e validação de host](https://man.openbsd.org/ssh_config#StrictHostKeyChecking)
- [Endereços dos runners GitHub](https://docs.github.com/en/actions/reference/runners/github-hosted-runners#ip-addresses)
- [Instalação .NET no Ubuntu](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install)
