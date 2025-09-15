# Configuração do Projeto - API de Antecipação de Valores

## 🛠️ Scripts de Configuração

### 1. Script de Criação da Solução (create-solution.sh)
```bash
#!/bin/bash

echo "🚀 Criando solução Antecipacao..."

# Criar solução
dotnet new sln -n Antecipacao -o src

# Criar projetos
echo "📁 Criando projetos..."
dotnet new classlib -n Antecipacao.Domain -o src/Domain
dotnet new classlib -n Antecipacao.Application -o src/Application
dotnet new classlib -n Antecipacao.Infrastructure -o src/Infrastructure
dotnet new webapi -n Antecipacao.WebAPI -o src/WebAPI
dotnet new xunit -n Antecipacao.Tests -o src/Tests

# Adicionar projetos à solução
echo "🔗 Adicionando projetos à solução..."
dotnet sln add src/Domain/Antecipacao.Domain.csproj
dotnet sln add src/Application/Antecipacao.Application.csproj
dotnet sln add src/Infrastructure/Antecipacao.Infrastructure.csproj
dotnet sln add src/WebAPI/Antecipacao.WebAPI.csproj
dotnet sln add src/Tests/Antecipacao.Tests.csproj

# Configurar referências entre projetos
echo "🔗 Configurando referências..."
dotnet add src/Application/Antecipacao.Application.csproj reference src/Domain/Antecipacao.Domain.csproj
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj reference src/Domain/Antecipacao.Domain.csproj
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj reference src/Infrastructure/Antecipacao.Infrastructure.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/WebAPI/Antecipacao.WebAPI.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/Application/Antecipacao.Application.csproj
dotnet add src/Tests/Antecipacao.Tests.csproj reference src/Infrastructure/Antecipacao.Infrastructure.csproj

echo "✅ Solução criada com sucesso!"
echo "📂 Estrutura de pastas:"
tree -I 'bin|obj|*.user' -a
```

### 2. Script de Instalação de Pacotes (install-packages.sh)
```bash
#!/bin/bash

echo "📦 Instalando pacotes NuGet..."

# Application Layer
echo "Instalando pacotes para Application..."
dotnet add src/Application/Antecipacao.Application.csproj package FluentValidation --version 11.8.1
dotnet add src/Application/Antecipacao.Application.csproj package MediatR --version 12.2.0
dotnet add src/Application/Antecipacao.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0

# Infrastructure Layer
echo "Instalando pacotes para Infrastructure..."
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package BCrypt.Net-Next --version 4.0.3
dotnet add src/Infrastructure/Antecipacao.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt --version 7.0.3
cd ..

# WebAPI Layer
echo "Instalando pacotes para WebAPI..."
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.AspNetCore.OpenApi --version 8.0.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Swashbuckle.AspNetCore --version 6.5.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add src/WebAPI/Antecipacao.WebAPI.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
cd ..

# Tests
echo "Instalando pacotes para Tests..."
dotnet add src/Tests/Antecipacao.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add src/Tests/Antecipacao.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add src/Tests/Antecipacao.Tests.csproj package FluentAssertions --version 6.12.0
dotnet add src/Tests/Antecipacao.Tests.csproj package Moq --version 4.20.69
dotnet add src/Tests/Antecipacao.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0

echo "✅ Pacotes instalados com sucesso!"
```

### 3. Script de Build e Test (build-and-test.sh)
```bash
#!/bin/bash

echo "🔨 Buildando solução..."

# Restaurar dependências
dotnet restore src/Antecipacao.sln

# Build da solução
dotnet build src/Antecipacao.sln

if [ $? -eq 0 ]; then
    echo "✅ Build realizado com sucesso!"
    
    echo "🧪 Executando testes..."
    dotnet test src/Antecipacao.sln --verbosity normal
    
    if [ $? -eq 0 ]; then
        echo "✅ Todos os testes passaram!"
    else
        echo "❌ Alguns testes falharam!"
        exit 1
    fi
else
    echo "❌ Build falhou!"
    exit 1
fi
```

## 📋 Arquivos de Configuração

### 1. .gitignore
```gitignore
# Build results
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio
.vs/
*.user
*.userosscache
*.sln.docstates

# User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# MSTest test Results
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*

# NUnit
*.VisualState.xml
TestResult.xml
nunit-*.xml

# .NET Core
project.lock.json
project.fragment.lock.json
artifacts/

# StyleCop
StyleCopReport.xml

# Files built by Visual Studio
*_i.c
*_p.c
*_h.h
*.ilk
*.meta
*.obj
*.iobj
*.pch
*.pdb
*.ipdb
*.pgc
*.pgd
*.rsp
*.sbr
*.tlb
*.tli
*.tlh
*.tmp
*.tmp_proj
*_wpftmp.csproj
*.log
*.vspscc
*.vssscc
.builds
*.pidb
*.svclog
*.scc

# Chutzpah Test files
_Chutzpah*

# Visual C++ cache files
ipch/
*.aps
*.ncb
*.opendb
*.opensdf
*.sdf
*.cachefile
*.VC.db
*.VC.VC.opendb

# Visual Studio profiler
*.psess
*.vsp
*.vspx
*.sap

# Visual Studio Trace Files
*.e2e

# TFS 2012 Local Workspace
$tf/

# Guidance Automation Toolkit
*.gpState

# ReSharper is a .NET coding add-in
_ReSharper*/
*.[Rr]e[Ss]harper
*.DotSettings.user

# TeamCity is a build add-in
_TeamCity*

# DotCover is a Code Coverage Tool
*.dotCover

# AxoCover is a Code Coverage Tool
.axoCover/*
!.axoCover/settings.json

# Coverlet is a free, cross platform Code Coverage Tool
coverage*.json
coverage*.xml
coverage*.info

# Visual Studio code coverage results
*.coverage
*.coveragexml

# NCrunch
_NCrunch_*
.*crunch*.local.xml
nCrunchTemp_*

# MightyMoose
*.mm.*
AutoTest.Net/

# Web workbench (sass)
.sass-cache/

# Installshield output folder
[Ee]xpress/

# DocProject is a documentation generator add-in
DocProject/buildhelp/
DocProject/Help/*.HxT
DocProject/Help/*.HxC
DocProject/Help/*.hhc
DocProject/Help/*.hhk
DocProject/Help/*.hhp
DocProject/Help/Html2
DocProject/Help/html

# Click-Once directory
publish/

# Publish Web Output
*.[Pp]ublish.xml
*.azurePubxml
# Note: Comment the next line if you want to checkin your web deploy settings,
# but database connection strings (with potential passwords) will be unencrypted
*.pubxml
*.publishproj

# Microsoft Azure Web App publish settings. Comment the next line if you want to
# checkin your Azure Web App publish settings, but sensitive information contained
# in these files may be visible to others.
*.azurePubxml

# Microsoft Azure Build Output
csx/
*.build.csdef

# Microsoft Azure Emulator
ecf/
rcf/

# Windows Store app package directories and files
AppPackages/
BundleArtifacts/
Package.StoreAssociation.xml
_pkginfo.txt
*.appx
*.appxbundle
*.appxupload

# Visual Studio cache files
# files ending in .cache can be ignored
*.[Cc]ache
# but keep track of directories ending in .cache
!?*.[Cc]ache/

# Others
ClientBin/
~$*
*~
*.dbmdl
*.dbproj.schemaview
*.jfm
*.pfx
*.publishsettings
orleans.codegen.cs

# Including strong name files can present a security risk
# (https://github.com/github/gitignore/pull/2483#issue-259490424)
#*.snk

# Since there are multiple workflows, uncomment the next line to ignore bower_components
# (https://github.com/github/gitignore/pull/1529#issuecomment-104372622)
#bower_components/

# RIA/Silverlight projects
Generated_Code/

# Backup & report files from converting an old project file
# to a newer Visual Studio version. Backup files are not needed,
# because we have git ;-)
_UpgradeReport_Files/
Backup*/
UpgradeLog*.XML
UpgradeLog*.htm
CDF_UpgradeLog*.txt

# SQL Server files
*.mdf
*.ldf
*.ndf

# Business Intelligence projects
*.rdl.data
*.bim.layout
*.bim_*.settings
*.rptproj.rsuser
*- [Bb]ackup.rdl
*- [Bb]ackup ([0-9]).rdl
*- [Bb]ackup ([0-9][0-9]).rdl

# Microsoft Fakes
FakesAssemblies/

# GhostDoc plugin setting file
*.GhostDoc.xml

# Node.js Tools for Visual Studio
.ntvs_analysis.dat
node_modules/

# Visual Studio 6 build log
*.plg

# Visual Studio 6 workspace options file
*.opt

# Visual Studio 6 auto-generated workspace file (has which files are open etc.)
*.vbw

# Visual Studio LightSwitch build output
**/*.HTMLClient/GeneratedArtifacts
**/*.DesktopClient/GeneratedArtifacts
**/*.DesktopClient/ModelManifest.xml
**/*.Server/GeneratedArtifacts
**/*.Server/ModelManifest.xml
_Pvt_Extensions

# Paket dependency manager
.paket/paket.exe
paket-files/

# FAKE - F# Make
.fake/

# CodeRush personal settings
.cr/personal

# Python Tools for Visual Studio (PTVS)
__pycache__/
*.pyc

# Cake - Uncomment if you are using it
# tools/**
# !tools/packages.config

# Tabs Studio
*.tss

# Telerik's JustMock configuration file
*.jmconfig

# BizTalk build output
*.btp.cs
*.btm.cs
*.odx.cs
*.xsd.cs

# OpenCover UI analysis results
OpenCover/

# Azure Stream Analytics local run output
ASALocalRun/

# MSBuild Binary and Structured Log
*.binlog

# NVidia Nsight GPU debugger configuration file
*.nvuser

# MFractors (Xamarin productivity tool) working folder
.mfractor/

# Local History for Visual Studio
.localhistory/

# BeatPulse healthcheck temp database
healthchecksdb

# Backup folder for Package Reference Convert tool in Visual Studio 2017
MigrationBackup/

# Ionide (cross platform F# VS Code tools) working folder
.ionide/

# Fody - auto-generated XML schema
FodyWeavers.xsd

# VS Code files for those working on multiple tools
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json
*.code-workspace

# Local History for Visual Studio Code
.history/

# Windows Installer files from build outputs
*.cab
*.msi
*.msix
*.msm
*.msp

# JetBrains Rider
.idea/
*.sln.iml
```

### 2. src/WebAPI/appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=:memory:"
  },
  "AntecipacaoSettings": {
    "TaxaAntecipacao": 0.05,
    "ValorMinimo": 100.00
  },
  "DatabaseSettings": {
    "UseSequentialIds": true,
    "IdGenerationStrategy": "Identity"
  },
  "JwtSettings": {
    "Secret": "uA3EBwUy5RkLS6QEm2Mu7T8+7j1Ki9IBU5SKzTyNWpE=",
    "Issuer": "AntecipacaoWebAPI",
    "Audience": "AntecipacaoWebAPIUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 3. src/WebAPI/appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=antecipacao.db"
  },
  "DatabaseSettings": {
    "UseSequentialIds": true,
    "IdGenerationStrategy": "Identity"
  },
  "JwtSettings": {
    "Secret": "Z+0Zptxmv3yEnx1QjHPcr+NDdD6ZyUMZihjE4sSyKxY=",
    "Issuer": "AntecipacaoWebAPI-Dev",
    "Audience": "AntecipacaoWebAPIUsers-Dev",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  }
}
```

## 🚀 Comandos de Desenvolvimento

### Setup Inicial
```bash
# 1. Tornar scripts executáveis
chmod +x *.sh

# 2. Executar criação da solução
./create-solution.sh

# 3. Instalar pacotes
./install-packages.sh

# 4. Build e teste
./build-and-test.sh
```

### Desenvolvimento Diário
```bash
# Executar aplicação
dotnet run --project src/WebAPI/Antecipacao.WebAPI.csproj

cd src/

# Executar testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Build da solução
dotnet build

# Limpar e rebuildar
dotnet clean && dotnet build
```

### Database
```bash
# Adicionar migration
dotnet ef migrations add InitialCreate --project src/Infrastructure/Antecipacao.Infrastructure.csproj --startup-project src/WebAPI/Antecipacao.WebAPI.csproj

# Atualizar database
dotnet ef database update --project src/Infrastructure/Antecipacao.Infrastructure.csproj --startup-project src/WebAPI/Antecipacao.WebAPI.csproj

# Remover migration
dotnet ef migrations remove --project src/Infrastructure/Antecipacao.Infrastructure.csproj --startup-project src/WebAPI/Antecipacao.WebAPI.csproj
```

## 📊 Monitoramento e Logs

### Health Checks
```csharp
// Adicionar em Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AntecipacaoDbContext>();

app.MapHealthChecks("/health");
```

### Logging Estruturado
```csharp
// Configurar Serilog (opcional)
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
```

## 🔧 Configurações de Desenvolvimento

### VS Code Settings
```json
{
  "dotnet.defaultSolution": "src/Antecipacao.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true
  }
}
```

### Launch Configuration
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/WebAPI/Antecipacao.WebAPI.csproj/bin/Debug/net8.0/Antecipacao.WebAPI.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/WebAPI",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```
