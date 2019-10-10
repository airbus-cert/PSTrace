# Minimum CMake required
cmake_minimum_required(VERSION 3.11)

# Project

project(pstrace LANGUAGES C CXX CSharp VERSION 1.0.1)

# Options

option(PSTRACE_BUILD_TESTS "Build tests" ON)

# Config

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# define cmake dir
set(PSTRACE_CMAKE_DIR "${CMAKE_SOURCE_DIR}/cmake")

if (${PSTRACE_BUILD_TESTS})
    enable_testing()
endif()

SET(EXECUTABLE_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin)
SET(LIBRARY_OUTPUT_PATH ${CMAKE_BINARY_DIR}/bin)

# Tree processing

add_subdirectory(pstrace)
add_subdirectory(minhook)
add_subdirectory(install)


# Summary

message(STATUS "Configuration summary")
message(STATUS "-- Project name                 : ${PROJECT_NAME}")
message(STATUS "-- Project version              : ${PROJECT_VERSION}")


# CPack part
set(CPACK_GENERATOR "WIX")
set(CPACK_WIX_UPGRADE_GUID "BF20CE5E-7F7C-401D-8F7C-AB45E8D170E6")
set(CPACK_WIX_UNINSTALL "1")
set(CPACK_WIX_PATCH_FILE "${CMAKE_CURRENT_SOURCE_DIR}/install/wix/patch.xml")
include(CPack)