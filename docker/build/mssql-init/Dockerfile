# escape=`

ARG PARENT_IMAGE
ARG HEADLESS_SERVICES_IMAGE

FROM ${HEADLESS_SERVICES_IMAGE} AS headless_services
FROM ${PARENT_IMAGE}

ENV SQL_SERVER='mssql' `
    SQL_ADMIN_LOGIN='sa' `
    SQL_ADMIN_PASSWORD='SCQV9E91bRvN8w6dUxUugbp-8z1yiyVEFnPvcNv8favQ5FmGluL66I-MsVAH' `
    SITECORE_ADMIN_PASSWORD='b' `
    POST_DEPLOYMENT_WAIT_PERIOD='300'

COPY --from=headless_services C:\module\db C:\resources\jss

HEALTHCHECK --interval=5s --timeout=30s --start-period=60s --retries=3 CMD ["powershell", "-NoLogo", "-Command", "if ([System.Environment]::GetEnvironmentVariable('DatabasesDeploymentStatus', 'Machine') -eq 'Complete') { exit 0 } else { exit 1}"]
