<h1 align="center">
    vke.net
    <br>
    Vulkan Engine for .NET
    <br>
<p align="center">
  <a href="https://www.nuget.org/packages/vke"><img src="https://buildstats.info/nuget/vke"></a>
  <a href="https://travis-ci.org/jpbruyere/vke.net">
      <img src="https://img.shields.io/travis/jpbruyere/vke.net.svg?&logo=travis&logoColor=white">
  </a>
  <a href="https://ci.appveyor.com/project/jpbruyere/vke-net">
	<img src="https://img.shields.io/appveyor/ci/jpbruyere/vke-net?label=Windows&logo=appveyor&logoColor=lightgrey">
  </a>
  <a href="https://www.paypal.me/GrandTetraSoftware">
    <img src="https://img.shields.io/badge/Donate-PayPal-green.svg">
  </a>
  <a href="https://gitter.im/CSharpRapidOpenWidgets?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge">
    <img src="https://badges.gitter.im/CSharpRapidOpenWidgets.svg">
  </a>
</p>
</h1>

<p align="center">
  <a href="https://github.com/jpbruyere/vke.net/blob/master/samples/pbr/screenshot.png">
    <kbd><img src="https://raw.githubusercontent.com/jpbruyere/vke.net/master/samples/pbr/screenshot.png" height="300"></kbd>
  </a>
   <br>adaptation of the gltf PBR sample from Sacha Willems</br>
</p>

# Presentation
`vke.net` (_vulkan engine for .net_) is a vulkan abstraction layer writen in **c#** composed of high level classes encapsulating [vulkan](https://www.khronos.org/vulkan/) objects and commands with `IDispose` model and **reference counting**.

`vke.net` use autogenerated [vk.net](https://github.com/jpbruyere/vk.net) library for low level binding to vulkan and [GLFW](https://www.glfw.org/) handles the default windowing system.

`vke.net` aims to provide a simple api for all common vulkan tasks, ideal to quickly prototype vulkan applications, but fits also the needs to build complete application or game.

To see `vke.net` in action check [vkChess.net](https://github.com/jpbruyere/vkChess.net), and to start a vulkan application with an integrated [GUI](https://github.com/jpbruyere/Crow), start with [VkCrowWindow](https://github.com/jpbruyere/VkCrowWindow).

Use the `download_datas.sh` script for downloading sample's datas.

vke is in beta development stage.

# Requirements
- [GLFW](https://www.glfw.org/) if you use the `VkWindow` class.
- [Vulkan Sdk](https://www.lunarg.com/vulkan-sdk/), **glslc** has to be in the path.

`vke.net` supports `netcoreapp3.0`.

# Tutorials

|                    Title                     |                    Screen shots                    |
| :------------------------------------------: | :------------------------------------------------: |
| [ClearScreen](samples/ClearScreen/README.md) | ![screenshot](samples/screenShots/ClearScreen.png) |
|    [Triangle](samples/Triangle/README.md)    |  ![screenshot](samples/screenShots/Triangle.png)   |
|    [Textured](samples/Textured/README.md)    |  ![screenshot](samples/screenShots/Textured.png)   |

# Contributing

See the [contribution guide](https://github.com/jpbruyere/vke.net/blob/master/CONTRIBUTING.md) for more information.

Join us on [gitter](https://gitter.im/CSharpRapidOpenWidgets) for any question.

# Features

- physicaly based rendering, direct and deferred
- glTF 2.0
- ktx image loading.
- Memory pools


