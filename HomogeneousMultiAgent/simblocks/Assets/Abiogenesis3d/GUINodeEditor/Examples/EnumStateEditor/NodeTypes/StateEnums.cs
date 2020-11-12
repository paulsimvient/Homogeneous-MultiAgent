using System.Collections.Generic;
using Type = System.Type;
using Activator = System.Activator;

public static class StateEnums {
    
    public static List<Type> enumTypes = new List<Type> () {
        typeof(Fruits),
        typeof(Axies),
        typeof(Seassons),
        typeof(Animals),
        typeof(Directions),
        typeof(Instruments)
    };

    public static Dictionary<string, object> GetInitialState () {
        Dictionary<string, object> initialStateDict = new Dictionary<string, object> () {};
        foreach (Type enumType in enumTypes)
            initialStateDict.Add (enumType.ToString(), Activator.CreateInstance (enumType));
        return initialStateDict;
    }

    #region enums
    public enum Fruits {
        None,
        Banana,
        Orange,
        Apple,
        Cherry,
        Ananas,
    }
    public enum Axies {
        None,
        X_axis,
        Y_axis,
        Z_axis,
    }
    public enum Seassons {
        None,
        Summer,
        Autumn,
        Winter,
        Spring,
    }
    public enum Animals {
        None,
        Wombat,
        Sitatunga,
        Tapir,
    }
    public enum Directions {
        None,
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down,
    }
    public enum Instruments {
        None,
        Guitar,
        Drums,
        Piano,
        Bass,
    }
    #endregion
}
