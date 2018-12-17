# Guanaco
A [Grasshopper](https://www.grasshopper3d.com/) plugin linking [Rhino](https://www.rhino3d.com/) with [Gmsh](http://gmsh.info/) mesher and [Calculix](http://www.calculix.de/) FE package, developed by [Tentech](https://tentech.nl/). The current implementation covers 1D & 2D elements (bars, panels) under structural loads.

<img src="https://github.com/PawelBee/Guanaco/blob/master/Wiki/Images/Others/Pillow.png" height="250"> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; <img src="https://github.com/PawelBee/Guanaco/blob/master/Wiki/Images/Others/SampleBuilding.png" height="250">

## Prerequisites ##
The plugin has been built with following versions of third party software:
- Rhino 5
- Grasshopper 0.9.76
- Calculix 2.14 (most/all functionalities should work with the versioh one available on the official [website](http://www.calculix.de/))
- Gmsh 3.01

## Compilation ##
The code has been built with Visual Studio 2015 using .NET4.5 framework. It should compile once the Grasshopper-related references are sorted out. It is recommeneded to change the post-build actions in the properties to copy the libraries directly into relevant Grasshopper libraries folder.

## How to use ##
The Wiki is available [here](https://github.com/PawelBee/Guanaco/wiki).

## Contribute ##
Guanaco is a Tentech open-source project and would be nothing without its community. You can freely fork from the main repo or create branches & submit your own code to the project via Github [pull request](https://help.github.com/articles/using-pull-requests).

## License ##
Guanaco is licensed under the [MIT](https://opensource.org/licenses/MIT) license. It also uses a number of third party libraries, some with different licenses.
