; from http://www.autohotkey.com/docs/Tutorial.htm

$F1::  ; Make the F1 key into a hotkey (the $ symbol facilitates the "P" mode of GetKeyState below).
Loop  ; Since no number is specified with it, this is an infinite loop unless "break" or "return" is encountered inside.
{
    if not GetKeyState("F1", "P")  ; If this statement is true, the user has physically released the F1 key.
        break  ; Break out of the loop.
    ; Otherwise (since the above didn't "break"), keep clicking the mouse.
    Click  ; Click the left mouse button at the cursor's current position.
}
return