# escape=`

ARG PARENT_IMAGE
ARG SOLUTION_IMAGE
ARG TOOLS_IMAGE
ARG MANAGEMENT_SERVICES_IMAGE
ARG HEADLESS_SERVICES_IMAGE

FROM ${SOLUTION_IMAGE} AS solution
FROM ${TOOLS_IMAGE} AS tools
FROM ${MANAGEMENT_SERVICES_IMAGE} AS management_services
FROM ${HEADLESS_SERVICES_IMAGE} AS headless_services
FROM ${PARENT_IMAGE}

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ARG BUILD_CONFIGURATION='Release'

ENV SITECORE_LICENSE_LOCATION='C:\license\license.xml' `
    Sitecore_ConnectionStrings_Core='Data Source=mssql;Initial Catalog=Sitecore.Core;User ID=sa;Password=SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    Sitecore_ConnectionStrings_Security='Data Source=mssql;Initial Catalog=Sitecore.Core;User ID=sa;Password=SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    Sitecore_ConnectionStrings_Master='Data Source=mssql;Initial Catalog=Sitecore.Master;User ID=sa;Password=SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    Sitecore_ConnectionStrings_Web='Data Source=mssql;Initial Catalog=Sitecore.Web;User ID=sa;Password=SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    Sitecore_ConnectionStrings_ExperienceForms='Data Source=mssql;Initial Catalog=Sitecore.ExperienceForms;User ID=sa;Password=SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    Sitecore_ConnectionStrings_Solr.Search='http://solr:8983/solr' `
    MEDIA_REQUEST_PROTECTION_SHARED_SECRET='4IDZxWd41uFe4_7drK_pnO9lzDLTgtK-L-gEx3KNLMzuwzEPEJpHX9G4HZtK' `
    Sitecore_AppSettings_Telerik.AsyncUpload.ConfigurationEncryptionKey='QCkAzV6c7q1bXGcdy8uFkJ9JC7Ns0nL00vlAo0eKZ9Mcn-zvbX-LmgdpnCJf' `
    Sitecore_AppSettings_Telerik.Upload.ConfigurationHashKey='gs4gZxwuW1POyjbHrYiL7SlZlM5kDtuNWGo2-XuGWU--cCPyD53keInlnTgarNTVXRwbh4anM5z0bNB6q8dBZBTKvK15yuLKpyjh' `
    Sitecore_AppSettings_Telerik.Web.UI.DialogParametersEncryptionKey='dV-9rMwXv-eiiZzN9XqL1Z6tZvQeYakvIeipnjrAHOhVYFSjtfZnzmOiqHk7iVqkhJpjHZijmjGeljq1Yq6Taj5vEZ6BMU64kCQp' `
    Sitecore_AppSettings_exmEnabled:define='no'

WORKDIR C:\inetpub\wwwroot

COPY --from=tools C:\tools C:\tools
COPY --from=management_services C:\module\cm\content C:\inetpub\wwwroot
COPY --from=headless_services C:\module\cm\content C:\inetpub\wwwroot
COPY --from=headless_services C:\module\tools C:\module\tools
RUN C:\module\tools\Initialize-Content.ps1 -TargetPath C:\inetpub\wwwroot; Remove-Item -Path C:\module -Recurse -Force;

RUN Remove-Item -Path '.\App_Config\Sitecore\ItemWebApi' -Recurse -Force; `
    Remove-Item -Path '.\App_Config\Sitecore\Update' -Recurse -Force; `
    Remove-Item -Path '.\App_Config\Sitecore\DetectionServices.Location' -Recurse -Force; `
    Remove-Item -Path '.\App_Config\Sitecore\DeviceDetection.Client' -Recurse -Force; `
    Remove-Item -Path '.\App_Config\Sitecore\Buckets' -Recurse -Force; `
    Remove-Item -Path '.\App_Config\Sitecore\MVC.DeviceSimulator' -Recurse -Force;

COPY --from=solution /artifacts/platform/ ./
RUN & 'C:\tools\scripts\Invoke-XdtTransform.ps1' -Path '.\Web.config' -XdtPath '.\Web.Common.config.xdt'; `
    & 'C:\tools\scripts\Invoke-XdtTransform.ps1' -Path '.\Web.config' -XdtPath ('.\Web.{0}.config.xdt' -f $env:BUILD_CONFIGURATION); `
    Remove-Item -Path '.\\Web.*.xdt' -Force;
