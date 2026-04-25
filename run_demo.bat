@echo off
chcp 65001 > nul
echo ==================================================
echo       AUTOTUREL: REFERENCE CALL / ЭТАЛОННЫЙ ВЫЗОВ
echo ==================================================

:: [RU] X=1200 (дистанция), Y=15 (цель смещена влево), Z=50 (высота)
:: [EN] X=1200 (distance), Y=15 (target offset left), Z=50 (altitude)
set TARGET_POS=--target-pos 1200,15,50

:: [RU] 20 м/с назад (по оси X)
:: [EN] 20 m/s backwards (along X axis)
set TARGET_VEL=--target-vel -20,0,0

:: [RU] 20 итераций, макс. время 12 сек.
:: [EN] 20 iterations, max time 12 sec.
set PARAMS=--iters 20 --max-time 12 --v0 815

:: [RU] Флаги отладки и записи
:: [EN] Debug and Logging flags
set FLAGS=--csv --debug

dotnet run -- %TARGET_POS% %TARGET_VEL% %PARAMS% %FLAGS%

echo.
echo --------------------------------------------------
echo [RU] Проверьте папку Reports для результатов.
echo [EN] Check Reports folder for results.
echo --------------------------------------------------
pause
