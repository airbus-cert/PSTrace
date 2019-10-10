
set(MINHOOK_SRC
	minhook/src/hde/hde32.c
	minhook/src/hde/hde32.h
	minhook/src/hde/hde64.c
	minhook/src/hde/hde64.h
	minhook/src/hde/hde64.h
	minhook/src/hde/pstdint.h
	minhook/src/hde/table32.h
	minhook/src/hde/table64.h
	minhook/src/buffer.c
	minhook/src/buffer.h
	minhook/src/hook.c
	minhook/src/trampoline.c
	minhook/src/trampoline.h
	minhook/dll_resources/MinHook.def
)

add_library(MinHook SHARED ${MINHOOK_SRC})
add_compile_definitions(UNICODE)

install(TARGETS MinHook DESTINATION .)
