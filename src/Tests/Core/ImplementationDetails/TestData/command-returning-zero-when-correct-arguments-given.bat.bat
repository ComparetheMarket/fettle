@echo off

if "%1%" equ "a" (	
	if "%2%" equ "b" (		
		if "%3%" equ "c" (
			exit /b 0
		)
	)
)

exit /b 1