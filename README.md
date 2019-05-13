# AR Twister
# Team: Tae Jin Kim, Anurag Madan, Rebecca Nicacio

Our three-person team developed an Augmented Reality version of the game, Twister, whose Unity-based environment can be visualized through the powerful UI System, the Microsoft HoloLens.

We also made use of the system’s build-in hand-tracking and the ARHoloToolkit package’s automated text-to-speech features for fun, interactive gameplay. Additional features, such as the Restart Button, which was used to facilitate replayability, a Score System, which was used to encourage friendly competition, and the Timer-Based Health System, which was used to introduce a sense of urgency, were also added to better reflect the current trend to faced-paced gaming.

# TIMELINE

3/28:

    Anurag found a git repository that used hand tracking

3/30:

    Tae Jin created an environment of a cube of spheres in Unity

4/3:

    Tae Jin updated the environment so that prefabs and tags were used based on sphere color

4/7:

    Tae Jin added two scripts:
    1) GameManager.cs: creates a cube that follows the hand
    2) HandBoxController.cs: collision with a sphere turns it white

4/14:

    Tae Jin updated the environment so that it is now 4 columns instead of 1 cube
    Tae Jin added a restart button, timer, instructions, and a game over

4/16:

    Anurag found ARHoloToolkit package for text-to-speech
    Anurag added text-to-speech feature for the instructions (HandBoxController.cs update)

4/17:

    Rebecca added random shuffling of spheres everytime the instructions are met (HandBoxController.cs update)

4/20:

    Anurag merged text-to-speech feature and random shuffling (HandBoxController.cs update)

4/23:

    Tae Jin moved random shuffling, text-to-speech, and instruction met checking to GameManager.cs

4/24:

    Rebecca updated the environment layout to fit what TAs wanted

4/27:

    Tae Jin added time-based health system, a point system, and made it so that you can only restart after it is game over

4/29:

    Rebecca made sure that restarting the game also reset the spheres' positions
    Anurag updated the environment layout to make it more fun to play in

5/1:

    Rebecca added background music while the game is playing, updated environment to be wider  
    
