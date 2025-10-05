Vagrant.configure("2") do |config|

  # ------------------------------
  # Ubuntu VM
  # ------------------------------
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "ubuntu-vm"

    ubuntu.vm.network "forwarded_port", guest: 50561, host: 50561

    ubuntu.vm.provision "shell", inline: <<-SHELL
      echo "===> Installing .NET SDK"
      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      sudo apt-get update
      sudo apt-get install -y dotnet-sdk-8.0

      echo "===> Adding BaGet as private NuGet source"
      dotnet nuget add source "http://localhost:50561/v3/index.json" -n BaGet3

      echo "===> Installing your NuGet package locally"
      dotnet tool install --global FinTrack --add-source "http://localhost:50561/v3/index.json"

      echo "===> Running installed tool"
      fintrack
    SHELL
  end

  # ------------------------------
  # Windows VM
  # ------------------------------
  # config.vm.define "windows" do |windows|
  #   windows.vm.box = "gusztavvargadr/windows-10"
  #   windows.vm.hostname = "windows-vm"

  #   windows.vm.network "private_network", ip: "192.168.56.20"

  #   windows.vm.provision "shell", inline: <<-SHELL
  #     echo "===> Installing .NET SDK (Windows)"
  #     choco install dotnet-8.0-sdk -y

  #     echo "===> Adding BaGet as private NuGet source"
  #     dotnet nuget add source "http://localhost:5000/v3/index.json" -n BaGet

  #     echo "===> Installing your NuGet package"
  #     dotnet tool install --global FinTrack.Tool --version 1.0.0 --add-source LocalBaGet

  #     echo "===> Running installed tool"
  #     FinTrack.Tool
  #   SHELL
  # end
end
