using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataItem
{
    public string Title;
    public string A;
    public string B;
    public string C;
    public string D;
    public int Answer;

    public DataItem(string title, string a, string b, string c, string d, string answer)
    {
        this.Title = title;
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
        this.Answer = StrToIndex(answer);
    }

    private int StrToIndex(string answer)
    {
        return answer.ToLower() switch
        {
            "a" => 1,
            "b" => 2,
            "c" => 3,
            "d" => 4,
            _ => 1
        };
    }
}