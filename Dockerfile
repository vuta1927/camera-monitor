FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 45892
EXPOSE 44350

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY camera monitor.csproj ./
RUN dotnet restore /camera monitor.csproj
COPY . .
WORKDIR /src/
RUN dotnet build camera monitor.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish camera monitor.csproj -c Release -o /app

FROM base AS final
RUN apt-get update && apt-get install -y openssh-server net-tools iputils-ping supervisor nano ffmpeg
RUN mkdir /var/run/sshd && mkdir logs
RUN useradd vuta && echo 'vuta:Echo@1927' | chpasswd
RUN echo adduser vuta sudo
RUN echo 'root:Echo@1927' | chpasswd
RUN sed -i 's/PermitRootLogin prohibit-password/PermitRootLogin yes/' /etc/ssh/sshd_config
# SSH login fix. Otherwise user is kicked off after login
RUN sed 's@session\s*required\s*pam_loginuid.so@session optional pam_loginuid.so@g' -i /etc/pam.d/sshd
ENV NOTVISIBLE "in users profile"
RUN echo "export VISIBLE=now" >> /etc/profile
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["supervisord", "-c","/etc/supervisor/supervisord.conf"]
