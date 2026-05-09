#  API Ovni — ADS-B Tracker

![.NET Version](https://img.shields.io/badge/.NET-9.0-purple?style=flat&logo=dotnet)
![MongoDB](https://img.shields.io/badge/Database-MongoDB-47A248?style=flat&logo=mongodb&logoColor=white)
![Status](https://img.shields.io/badge/Status-Em_Desenvolvimento-yellow)
![License](https://img.shields.io/badge/License-MIT-blue)

API desenvolvida em **ASP.NET Core 9** para coleta, armazenamento e análise de dados ADS-B capturados por receptores (como Raspberry Pi).  
Projeto acadêmico com foco em **performance, segurança e arquitetura limpa**.

---

##  Funcionalidades

###  Segurança Robusta
- Autenticação via `X-Api-Key`
- Hash seguro (nenhuma chave armazenada em texto plano)

###  RBAC — Controle de Acesso
- **Admin:** leitura, escrita e configurações
- **User:** somente leitura

###  Gestão Automática de Armazenamento
- Serviço `LimpezaArmazenamentoService` remove voos antigos quando o tamanho máximo configurado é ultrapassado  
  *(configuração salva no banco em `ConfiguracaoLimpeza`)*

###  Alta Performance
- Conexão com MongoDB usando Singleton
- Índices otimizados para leituras rápidas

###  Documentação Automática
- Swagger totalmente integrado

---

##  Tecnologias Utilizadas
- **Back-end:** ASP.NET Core 9 (C#)
- **Banco:** MongoDB
- **Documentação:** Swagger (Swashbuckle)
- **Servidor:** Kestrel

---

#  Instalação e Configuração

## 1. Configuração do Banco de Dados (MongoDB)

### 1.1 Pré-requisitos
- MongoDB Community Server  
- MongoDB Compass (Recomendado)

### 1.2 Instalação
1. Baixe o **MongoDB Community Server**.
2. Marque a opção **“Install MongoDB Compass”**.
3. Mantenha habilitado: **“Run service as Network Service user”**.

### 1.3 Criando o Banco e Coleções
1. Abra o **MongoDB Compass**.
2. Conecte usando: `mongodb://localhost:27017`

3. Crie um Database:
   - **`nomeBanco`**
4. Crie as coleções:
   - `ovniData`
   - `apiKeys`
   - `ConfiguracaoLimpeza`

### 1.4 Script de Inicialização (Obrigatório)
Abra o **MongoSH** e execute:

```javascript
use nomeBanco

// 1. Índices de Performance e Unicidade
db.apiKeys.createIndex({ "email": 1 }, { unique: true })
db.ovniData.createIndex({ "data": 1 })

// 2. Configuração Inicial de Limpeza (200MB)
db.ConfiguracaoLimpeza.insertOne({
  "_id": "config_adsb",
  "descricao": "Configuração Padrão",
  "limiteMaximoBytes": 209715200,
  "percentualAlvoOcupacao": 0.9
})
```

Exemplo de `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDBSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RadarAereoDB"
  },
  "Authentication": {
    "AdminApiKey": "DEFINA_SUA_CHAVE_MESTRA_AQUI"
  }
}
```

```bash
# Restaurar pacotes e dependências
dotnet restore

# Iniciar o servidor
dotnet run
```

A API iniciará em: `https://localhost:7199`

## Documentação da API

Acesse o Swagger:

`https://localhost:7199/swagger`

Para acessar rotas protegidas no Swagger: clique em Authorize e cole sua `X-Api-Key`.

## Estrutura do Banco de Dados

O sistema utiliza 3 coleções principais.

### 1) Coleção `ovniData` — Telemetria ADS-B

Armazena dados das aeronaves:

| Campo | Descrição |
|---|---|
| _id | ObjectId |
| hex_id | Identificador ICAO |
| flight | Número do voo / Callsign |
| lat, lon | Coordenadas geográficas |
| alt_baro | Altitude barométrica |
| alt_geom	| Altitude geométrica (GPS) |
| ground_speed | Velocidade |
| indicated_air_speed |	Velocidade indicada (IAS) |
| true_air_speed	| Velocidade real (TAS) |
| squawk	| Código Squawk (identificação) |
| track	| Direção / Rumo em graus |
| emergency	| Estado de alerta ou emergência |
| data | Data/hora da captura (indexado) |




### 2) Coleção `apiKeys` — Controle de Acesso

| Campo | Descrição |
|---|---|
| _id | Hash SHA-256 da API Key |
| email | Usuário (índice único) |
| name | Nome completo |
| isAdmin | Booleano |
| createdAt | Data de criação |

As API Keys não são armazenadas em texto plano.

### 3) Coleção `ConfiguracaoLimpeza`

Configura o comportamento automático do serviço de limpeza.

| Campo | Descrição |
|---|---|
| _id | "config_adsb" |
| limiteMaximoBytes | Tamanho máximo permitido |
| percentualAlvoOcupacao | Percentual após limpeza |

