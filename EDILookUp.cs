using System;

public class EDILookUp
{
	
	public static string EDILookUp(string desired)
	{
        List<string[]> EDIValues = new List<string[]>
        {
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {},
            new string[] {}
        };

        string value = EDIValues.First(item=> item.Abreviation == desired).Value;

        return value;
	}
}
