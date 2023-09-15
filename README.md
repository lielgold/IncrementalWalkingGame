# Incremental walking game

An incremental game in which progress is achieved by walking with one's phone, rather than by clicking or waiting for time to pass. Made in Unity 2022.3.8f1, works on Android.

APK available [here](https://drive.google.com/file/d/1UG6YsVjIiUZ8mDxGyjo12jtoDYPqFW0J/view?usp=drive_link).

- Before api version 29 Android didn’t require user permission to use the accelerometer sensor (ACTIVITY_RECOGNITION permission). After that you need to ask for user permission.

- It takes a couple of seconds until the sensor starts to work. At first it returns 0, and then it gives the correct data.
