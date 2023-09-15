# Incremental walking game

Incremental game where walking progresse the game, rather than clicking or waiting for time to pass. Made in Unity, works on android.

APK available [here](https://drive.google.com/file/d/1UG6YsVjIiUZ8mDxGyjo12jtoDYPqFW0J/view?usp=drive_link).

- Android before api version 19 didn’t require user permission to use the accelerometer sensor (ACTIVITY_RECOGNITION permission). After that you need to ask user permission.

- It takes a couple of seconds until the sensor starts to work. At first it returns 0, and then it gives the correct data.
