@echo off
setlocal enabledelayedexpansion

rem Open StartAllPartitions.cmd file for writing
(
    echo @echo off

    rem Loop through hex values 00 to FF (0 to 255 in decimal)
    for /L %%H in (0,1,255) do (
        rem Convert decimal to two-digit hex
        set /a DECIMAL=%%H
        for /f "tokens=*" %%A in ('powershell -command "[convert]::ToString(!DECIMAL!, 16).PadLeft(2, '0')"') do (
            set HEX=%%A
        )

        rem Calculate corresponding port number starting from 7000
        set /a PORT=7000 + !DECIMAL!

        rem Output the line to start the partition
        echo start cmd /C StartPartition.cmd !HEX! !PORT!
    )

) > StartAllPartitions.cmd

echo StartAllPartitions.cmd generated successfully!
