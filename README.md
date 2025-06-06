# Engine

A simple, in development 3D game engine using OpenTK (OpenGL c# wrapper)

## Features
- OBJ + MTL mesh loading
- Object transformations, rotations and scaling
- Input handling
- Camera system
- Custom cubemap Skyboxes
- Resource manager

## Upcoming features
- BVH based collision detection system
- Improved camera functionality/modes
- Real time lighting
- Level editing system
- Simple animations

## Installation
Currently you need to clone this repository and run it in Visual Studio.
Nuget packages be provided in a future release.

## Usage
Before we start, it is recommended to have an assets folder in your project directory. This way you can easily reference your files.

To create a game window, we have to call Engine.Run, giving it the class we want to use for our game logic. WindowSettings lets you customize aspects of the window, such as its resolution, title, vsync, and whether it's fullscreen.

```csharp
using SampleGame.Engine.Core;

namespace SampleGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Windowsettings includes: width, height, name, vsync, fullscreen
            Engine.Core.Engine.Run<Game>(new WindowSettings(800, 600, "cool name", false, false));
        }
    }
}

```
The Game type referenced is the class that will interact with the IGame interface. This grants access to core window events that allow us to control our game. It has 5 mandatory methods that we must include for it to work properly. (OnLoad, OnUnload, OnRenderFrame, OnUpdateFrame, OnResize)

```csharp
public interface IGame
{
    void OnLoad();
    void OnUnload();
    void OnRenderFrame(FrameEventArgs args);
    void OnUpdateFrame(FrameEventArgs args);
    void OnResize(ResizeEventArgs e);
}
```

To render anything, we first have to load it into memory. Add the following using directive at the top of your file to access the engine's content system:

```csharp
using SampleGame.Engine.Content;
```

The loading can be done using the ResourceLoader class. With it we can load Wavefront files (OBJ, MTL):

```csharp
ResourceLoader.Instance.LoadWavefrontAsset(@"Assets\aFile.obj");
```

We can also load entire folders at once. Like this:

```csharp
ResourceLoader.Instance.LoadWavefrontFolder(@"Assets\aModelFolder\");
```
It is also possible to unload Wavefront files to free memory (Note unloading has to wait on GC so it's usually not instant). This is done in a similar way:

```csharp
void IGame.OnLoad()
{
    // Option 1
    ResourceLoader.Instance.UnloadWavefrontAsset("aFile.obj"); // Note to unload a file we only need to give it the filename

    // Option 2
    ResourceLoader.Instance.UnloadWavefrontFolder(@"Assets\aModelFolder\");
}
```
Loading a Skybox can be a bit more tedious. We need to provide a name for the Skybox, a folder path and 6 filenames for every face image of the Skybox. Here is an example:

```csharp
ResourceLoader.Instance.LoadSkybox("blue_sky", @"Assets\aSkyboxFolder\", "px.png", "nx.png", "py.png", "ny.png", "pz.png", "nz.png");
```
Now that we have our files loaded, we can create stuff with it in our code. Creating a model is simple, we need to provide it with a OBJ filename and a MTL filename from our loaded assets. After that the model needs to be initialized. Here is how we can do that:

Add the following using directive:
```csharp
using SampleGame.Engine.Core;
```

```csharp
Model model = new Model("aFileName.obj", "aFileName.mtl");
model.Initialize();
```
And this is how we can create a Skybox:

```csharp
Skybox skyBox = new Skybox("blue_sky");
```
Next we need a camera, the camera will allow us to navigate around the scene. Currently there is only one mode for the camera and that is "fly" mode. The only thing we need to specify when creating a camera is the starting position:

```csharp
Camera camera = new Camera(new Vector3(0, 0, -3));
camera.Fov = 90; // You can also change certain camera settings, like FOV
```
To actually render models and Skyboxes to the screen, we make use of the Engine class. Rendering should happen in OnRenderFrame, doing it somewhere else may lead to unexpected problems.

```csharp
void IGame.OnRenderFrame(FrameEventArgs args)
{
    Engine.Core.Engine.RenderSkybox(skyBox, camera);

    Engine.Core.Engine.RenderModel(model, camera);
}
```
At this point we have loaded files, and rendered them to the screen. But there is no way to move around the scene. To do this, we can use the movement and camera panning methods from our camera instance. We can also edit camera movement speed and mouse sensitivity. Game logic should go in OnUpdateFrame:

```csharp
void IGame.OnUpdateFrame(FrameEventArgs args)
{
    // Where 3f is the movement speed
    camera.HandleMovement(args, 3f);

    // And 0.3f is the sensitivity
    camera.HandleCamera(0.3f);
}

```
Now you should be able to move around, but you may notice that your model is much larger/smaller than expected. To change this we can use one of the many transformation/rotation/scaling methods. We simply call the method on our model, and the model will respond accordingly.

```csharp
model.SetPosition(-10, 0, 0);
model.Scale(0.5f);
model.RotateX(90);
```
These are just some examples, and you will find that there are many more available methods.

Finally, we can also handle keyboard and mouse input. We do this by accessing Keyboard and Mouse from the engine's WindowVariables. Using them is easy. Here is an example:

```csharp
KeyboardState keyboard = Engine.Core.Engine.WindowVariables.Keyboard;

if (keyboard.IsKeyDown(Keys.Up))
{
    model.TransformY(0.02f);
}
```

Your final game class might look something like this:

```csharp
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SampleGame.Engine.Content;
using SampleGame.Engine.Core;

namespace SampleGame
{
    class Game : IGame
    {
        Model model;
        Camera camera;
        Skybox skyBox;

        void IGame.OnLoad()
        {
            ResourceLoader.Instance.LoadWavefrontFolder(@"Assets\erato");
            ResourceLoader.Instance.LoadSkybox("blue_sky", @"Assets\Skybox\", "px.png", "nx.png", "py.png", "ny.png", "pz.png", "nz.png");

            model = new Model("erato.obj", "erato.mtl");
            model.Initialize();

            camera = new Camera(new Vector3(0, 0, -3));
            camera.Fov = 90;

            skyBox = new Skybox("blue_sky");

            model.SetPosition(-10, 0, 0);
            model.Scale(0.5f);

            ResourceLoader.Instance.UnloadWavefrontFolder(@"Assets\erato");
        }

        void IGame.OnUnload()
        {

        }

        void IGame.OnRenderFrame(FrameEventArgs args)
        {
            Engine.Core.Engine.RenderSkybox(skyBox, camera);

            Engine.Core.Engine.RenderModel(model, camera);
        }

        void IGame.OnUpdateFrame(FrameEventArgs args)
        {
            KeyboardState keyboard = Engine.Core.Engine.WindowVariables.Keyboard;

            if (keyboard.IsKeyDown(Keys.Up))
            {
                model.TransformY(0.02f);
            }

            camera.HandleMovement(args, 3f);
            camera.HandleCamera(0.3f);
        }

        void IGame.OnResize(ResizeEventArgs e)
        {

        }
    }
}

```

## Model format
Currently, only Wavefront models are supported (OBJ, MTL).

An OBJ file contains vertex data, and how to build the geometry of the model. Here is an example:

```obj
# Define the vertices
v 0.0 0.0 0.0
v 1.0 0.0 0.0
v 1.0 1.0 0.0
v 0.0 1.0 0.0
v 0.5 0.5 1.0

# Define faces
f 1 2 3
f 1 3 4
f 1 2 5
f 2 3 5
f 3 4 5
f 4 1 5
```
An MTL file contains material data, this incudes things like textures and material properties. Here is a very simple example of what that might look like:

```mtl
newmtl pyramidMaterial
Ka 0.2 0.2 0.2    # Ambient color
Kd 0.8 0.2 0.2    # Diffuse color
Ks 1.0 1.0 1.0    # Specular color
Ns 50.0           # Shininess
```

## Final notes
This project is still in development so expect bugs and inconsistencies.

## Known Bugs/Issues
- Not all Wavefront files like to load properly
- Transformation/rotation/scaling values must be very small
- High memory usage with certain models
- Slow loading with certain models (Use release mode for faster loading)
- While loading it may require you to press enter on the console (You know this when CPU utilisation for the program is at 0%)

## Screenshots

San Miguel
![test 6_5_2025 11_38_08 PM](https://github.com/user-attachments/assets/11cf6fe9-d996-4dbb-bc59-5c34303ff95f)

San Miguel
![test 6_5_2025 11_37_41 PM](https://github.com/user-attachments/assets/32ca0131-7842-4599-9e17-2b7c7d67e0a6)

Erato
![test 6_5_2025 11_18_44 PM](https://github.com/user-attachments/assets/e748faa4-4cdc-4f95-9d7b-fa4c773b3e01)

Models are sourced from Casual Effects (July.2017). Retrieved from https://casual-effects.com/data/


