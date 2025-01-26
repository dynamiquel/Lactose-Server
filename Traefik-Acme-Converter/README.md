# traefik-acme-converter
Converts ACME certificates from Traefik into separate `fullchain` and `privkey` files for each domain.

This repository contains a Docker container designed to watch a given path for an ACME (Let's Encrypt) JSON file. It automatically extracts and decodes the full chain certificates and private keys for all domains specified in the JSON file using `jq` and `base64`.

## Prerequisites

- Docker installed on your machine.
- `jq` and `base64` are used within the Docker container, so no need to install those.

## Build

Clone this repository and build the Docker image:

```bash
git clone https://github.com/shobuprime/traefik-acme-converter.git
cd traefik-acme-converter
docker build -t shobuprime/traefik-acme-converter .
```

## Usage

Run the Docker container:

```bash
docker run -v /path/to/watch:/traefik/certs -v /path/to/output:/app/output shobuprime/traefik-acme-converter
```

### Environment Variables

You can specify several environment variables to customize the behavior:

- `WATCH_DIR`: The directory to watch for the ACME JSON file. Defaults to `/traefik/certs`.
- `INTERVAL`: Time in seconds between each check. Defaults to `1800`.
- `PROVIDER`: jq path segment for your provider's certificates array. Defaults to `ACME`.
- `ACME_FILE_NAME`: Automatically constructed from the `PROVIDER` variable. Defaults to `ACME.json` (or `${PROVIDER}.json`).
- `OUTPUT_DIR`: The directory where the certificates and key files should be saved. Defaults to `/app/output`.

### Example with Environment Variables

```bash
docker run \
  -e WATCH_DIR=/custom/watch/dir \
  -e INTERVAL=600 \
  -e PROVIDER=customProvider \
  -e OUTPUT_DIR=/custom/output \
  -v /path/to/watch:/custom/watch/dir \
  -v /path/to/output:/custom/output \
  shobuprime/traefik-acme-converter
```

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.

---
