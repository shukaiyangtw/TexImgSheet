How to Use
----------


To illustrate with an example, the following four images are prepared:

| File name| Pictures | 
| --- | --- | 
| flame0.png | ![An example picture](example/flame0.png) | 
| flame1.png | ![Anexample picture](example/flame1.png) | 
| flame2.png | ![Anexample picture](example/flame2.png) | 
| flame3.png | ![Anexample picture](example/flame3.png) | 

Next, prepare a [flame.xml](example/flame.xml) file in the same directory, with the following contents:

```xml
<?xml version="1.0"?>
<sheet>
  <canvas rows="1" cols="4" cellwidth="512" cellheight="512" />
  <sprites>
    <img row="0" col="0" src="flame0.png" />
    <img row="0" col="1" src="flame1.png" />
    <img row="0" col="2" src="flame2.png" />
    <img row="0" col="3" src="flame3.png" />
  </sprites>
</sheet>
```

Drag this file onto the software screen below. You should see the information in the frame.xml file filled in, as shown in the figure below:

![The screen shot](images/screenshot.png)

Finally, click the "Export PNG Now..." button to get a frame.png as shown below:

![The result image](example/flame.png)