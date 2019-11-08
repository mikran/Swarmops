FROM ubuntu:xenial

# Install documented dependencies.
RUN apt-get update && apt-get install apt-transport-https -y

# Install undocumented? dependencies.
RUN apt-get update && apt-get install systemd wget gnupg -y

RUN wget -qO- http://packages.swarmops.com/swarmops-packages.gpg.key | apt-key add - && \
  apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
  echo deb http://packages.swarmops.com/ xenial contrib > /etc/apt/sources.list.d/swarmops.list && \
  echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial main" > /etc/apt/sources.list.d/mono-official-stable.list

RUN apt-get update && apt-get install swarmops-backend -y
