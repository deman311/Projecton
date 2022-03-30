# Projecton
Job Interview Project

### Day 🥇 29/03/22
So the things I did today were to read about ONNX, search for suitable datasets for my model, read some CNN notebooks to refresh my memory about CNN's,
then I took inspiration from dog-cat Kaggle competition notebooks and finally started writing the model.

Lots of training and retraining, tweaking parameters, testing basic stuff - mostly to refresh memory.

End result of the day:

![firstCNN](/firstDecentCNN.png "First Decent CNN") 

### Day 2️ 30/03/22
So today I played even further with the parameters, after also training the model on the validation set I've managed
to reach an accuracy of about 85% (worst case). Also, after going through a couple of test sets I'd say it is also pretty close to that score on the test.

I've continued reading about ONNX and started setting up the API in Visual Studio, I chose .NET Core 6.0. I had various bugs like not being able to send an input and recieve an output from the model. Also had to debug and learn how to cast and format different types like C# Tensor to other forms (with the First() method) that took some time.

Finally, I manged to set up a functioning API that loads an image from the local storage, injects it to the ONNX model and successfully returns a JSON with the correct label.

I think I'll take another day to tidy up some bits (like the Colab notebook - that needs some more annotation), and also I want to read about a some methods and syntaxs that are new to me as I didn't get the chance to work on API's that much.

P.S. The dog is my girlfriend's.