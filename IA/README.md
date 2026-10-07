# Executável da IA

A interface do sistema chama `executar_ia.exe` e não precisa encontrar nem instalar Python na máquina que executa o aplicativo. O executável contém o runtime Python e as bibliotecas usadas pela IA; o modelo e o CSV também são empacotados.

## Gerar o executável

Na máquina de desenvolvimento, instale Python 3 e execute no PowerShell:

```powershell
cd IA
.\build.ps1
```

O arquivo será criado em `IA/dist/executar_ia.exe`. Depois, compile ou publique `INTERFACE_POSTRATA`; o projeto copia o executável para a pasta `IA` ao lado do aplicativo. Distribua a pasta publicada completa.

O Python e o PyInstaller são necessários somente para gerar o executável. A máquina de destino precisa ser Windows compatível com a arquitetura do build. Como o empacotamento é `onefile`, a primeira inicialização pode levar alguns segundos.
