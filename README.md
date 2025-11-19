# ADS-B Tracker API - Servidor de Rastreamento Aéreo

[![Powered by ASP.NET Core](https://img.shields.io/badge/Tech-ASP.NET%20Core%209-blueviolet)](https://dotnet.microsoft.com/)
[![Database: MongoDB](https://img.shields.io/badge/Database-MongoDB-4EA94B)](https://www.mongodb.com/)
[![Security: API Key Auth](https://img.shields.io/badge/Security-Custom%20API%20Key-orange)](https://docs.microsoft.com/en-us/aspnet/core/)

Este projeto em desenvolvimento é um sistema desenvolvido em **ASP.NET Core 9** e utiliza **MongoDB** para armazenar dados de rastreamento aéreo. 

---

## Funcionalidades Principais Atuais

* **Autenticação (API Key):** Sistema de segurança baseado no cabeçalho `X-Api-Key`, validado pelo `ApiKeyAuthenticationHandler`.
* **Controle de Acesso (Roles):** Diferenciação entre privilégios **Admin** (Leitura, Escrita, Configuração) e **User** (Apenas Leitura).
* **Gerenciamento de Armazenamento:** Rotina automática (`LimpezaArmazenamentoService`) que remove os documentos mais antigos (`OvniData`) quando o limite de **bytes** é atingido.
* **Endpoints Seguros:** Geração de chaves públicas (`KeyRequestController`) e *endpoints* exclusivos para o Admin (`AdminController`).

---


## Guia Rápido de Instalação e Uso

### 1. Requisitos

* **SDK do .NET Core 9** ou superior.
* **MongoDB:** Uma instância rodando (local ou Atlas).
* **Certificado HTTPS:** Confirme que o certificado de desenvolvimento está instalado (`dotnet dev-certs https`).

### 2. Configuração (`appsettings.json`)

Abra o arquivo **`appsettings.json`** e configure os parâmetros essenciais:

* **String de Conexão:** Atualize o endereço do seu servidor MongoDB.
    ```json
    "ConnectionStrings": {
        "MongoDb": "mongodb://[SEU_IP_OU_HOST]/" 
    },
    ```
* **Nome do Banco de Dados:** pode ser alterado aqui.
    ```json
    "DataBaseName": "aviao",
    ```
* **Chave Mestra Admin:** Confirme ou altere a chave de acesso principal.
    ```json
    "Authentication": {
        "AdminApiKey": "XXXXXXXXXXX" 
    }
    ```

### 3. Como Rodar

Inicie o projeto pelo terminal ou IDE:

```bash
dotnet run