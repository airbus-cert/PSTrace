# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

set(GACINSTALLER_SRC
	src/GACInstaller.cs
)

add_executable(GACInstaller ${GACINSTALLER_SRC})
set_property(TARGET GACInstaller PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v2.0")
set_property(TARGET GACInstaller PROPERTY VS_DOTNET_REFERENCES
	"System.EnterpriseServices"
)

install(TARGETS GACInstaller DESTINATION .)