# Use an official Alpine as a parent image
FROM alpine:latest

# Install jq and other required packages
RUN apk add --no-cache jq coreutils

# Set the working directory in the container
WORKDIR /app

# Copy the shell script into the container
COPY watch-and-extract.sh /app

# Make the script executable
RUN chmod +x /app/watch-and-extract.sh

# Run the script when the container is started
CMD [ "/app/watch-and-extract.sh" ]
