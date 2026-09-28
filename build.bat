@echo off
chcp 65001 > nul
cd /d "%~dp0"

set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo [오류] 윈도우 기본 컴파일러 csc.exe 를 찾을 수 없습니다.
  pause
  exit /b 1
)

echo 시간표 화면보호기를 만드는 중...
"%CSC%" /nologo /codepage:65001 /target:winexe /out:Timetbl.scr /r:System.Windows.Forms.dll /r:System.Drawing.dll saver\Timetbl.cs
if errorlevel 1 (
  echo.
  echo [오류] 만들기에 실패했습니다. 위 메시지를 캡처해서 보내주세요.
  pause
  exit /b 1
)

echo.
echo 완료! 이 폴더에 Timetbl.scr 이 만들어졌습니다.
echo   1) Timetbl.scr 마우스 오른쪽 클릭 - [설치]
echo   2) 화면 보호기 설정 창에서 대기 시간 지정 후 [확인]
echo index.html 과 Timetbl.scr 은 같은 폴더에 두어야 합니다.
echo.
pause
