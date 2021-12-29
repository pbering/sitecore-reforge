# Sitecore Reforge

Bare-bones Sitecore XM environment for headless (cross platform) development.

This is the main repository, when you have [built and published the images](#build-and-share-images-on-a-private-registry), see [sitecoreops/sitecore-reforge-example-blackbox](http://github.com/sitecoreops/sitecore-reforge-example-blackbox) for example compose setup for consuming the images and [sitecoreops/sitecore-reforge-example-netcore](http://github.com/sitecoreops/sitecore-reforge-example-netcore) for a rendering host example.

## Goals

- Provide a Sitecore XM headless development environment with the smallest possible footprint in terms of compute, code and config.
- Rendering Host development can be done cross platform.
- Sitecore CLI can be used cross platform.

## Non-goals

- Production ready headless content delivery images.

## Scenarios

Which development setups does this approach enable?

| Host OS | CM                | Rendering Host         | CLI      | CLI CM connection |
| ------- | ----------------- | ---------------------- | -------- | ----------------- |
| Windows | Windows container | Host/Windows container | Host     | Direct  |
| Windows | Windows container | WSL/Linux container    | Host/WSL | Direct* |
| Windows | k8s/VM/other      | Host/Windows container | Host     | Direct* |
| Windows | k8s/VM/other      | WSL/Linux container    | Host/WSL | Direct* |
| macOS   | k8s/VM/other      | Host/Linux container   | Host     | Direct* |
| Linux   | k8s/VM/other      | Host/Linux container   | Host     | Direct* |

> \* The Sitecore CLI now supports HTTP (as of v4.1.0) using the `--insecure` flag when CM is running remotely (ie. non localhost address) without SSL.

## The Black Box XM

- Minimal XM with management and headless services installed.
- Only two containers running, the CM and SQL.
- Supports the Sitecore CLI *without* Identity Server.
- Solr is disabled by default.
- No SSL or reverse proxies, just simple port publishing.
- Default environment variables embedded in images, makes consumer compose files simpler and they can be overridden at runtime.
- Some useless(in this context) features removed/disabled such as "WebDAV", "Item Web API", "Device Detection", "Buckets", "Geo IP", "Item Cloning" etc.

### Sitecore CLI support without Sitecore Identity Server

- Authenticate with: `dotnet sitecore login --insecure --cm <BLACK BOX CM URL:PORT> --auth <BLACK BOX CM URL:PORT> --client-credentials true --allow-write true --client-id "sitecore\admin" --client-secret "b"` (notices that both `--auth` and `--cm` points to the **same** CM url)
- CLI authentication is implemented as simple pipelines, see [config](/src/Reforge.BlackBox/App_Config/Include/Reforge.CliSupport.config).

## Build and share images on a private registry

Run:

1. `$env:REGISTRY="<REGISTRY>/"` to set your target registry so images are tagged accordingly.
1. `docker login` or whatever is needed to authenticate to your target registry.
1. `docker-compose build`
1. `docker-compose push cm mssql mssql-init`

Check [http://github.com/sitecoreops/sitecore-reforge-example-blackbox](http://github.com/sitecoreops/sitecore-reforge-example-blackbox) for example usage.
