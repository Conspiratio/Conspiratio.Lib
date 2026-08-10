using Xunit;

// Der gesamte Spielzustand liegt in den statischen SW-Fassaden. Liefen Testklassen parallel, würden sie
// sich gegenseitig die Spielwelt unter den Füßen wegziehen – daher strikt nacheinander.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
