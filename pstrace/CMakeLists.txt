# Minimum CMake required
cmake_minimum_required(VERSION 3.11)
SET(CMAKE_CSharp_FLAGS "/platform:anycpu")

set(PSTRACE_SRC
	src/PSTrace.cs
	src/ScriptBlock.cs
	src/Pipeline.cs
	src/MinHook.cs
)

add_library(PSTrace SHARED ${PSTRACE_SRC})
set_property(TARGET PSTrace PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v2.0")
set_property(TARGET PSTrace PROPERTY VS_GLOBAL_SignAssembly "true")
set_property(TARGET PSTrace PROPERTY VS_GLOBAL_AssemblyOriginatorKeyFile "${CMAKE_CURRENT_SOURCE_DIR}/key/pstrace.snk")
set_property(TARGET PSTrace PROPERTY VS_DOTNET_REFERENCES
	"System"
)

install(TARGETS PSTrace DESTINATION .)