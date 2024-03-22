# forest-simulator
Forest Simulator based on Unity

The present simulator aims to generate forest scenes procedurally, using a general seed to ensure its repeatability. The Simulator works in Editor Mode. 


## How to use:

The following guide is a step-by-step basic tutorial. Feel free to explore the functionalities and adapt them to your needs. 

 1. In a new scene or in a preexisting one, create an empty object in the Hierarchy (right click, create empty object). We will name this object as Terraformer in this tutorial, but you can change it freely. Make sure in the Inspector that its transformation is in (x, y, z) = (0, 0, 0). 
 2. Add the script "Terraformer.cs" to the Terraformer object. After this, clicking in the Terraformer object will show in the a lot of customizable options. 
    1. In "General Seed" set the seed number you want to generate. Every seed generates a different forest scene.
    