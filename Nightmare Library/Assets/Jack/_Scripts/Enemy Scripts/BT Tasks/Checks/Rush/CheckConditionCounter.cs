using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConditionCounter : CheckCondition
{
    private int counter = 0;
    private int condition = 0;
    
    public enum EvalType { LESS, GREATER,  LESS_EQUAL, GREATER_EQUAL, EQUAL }
    private EvalType eval;

    public CheckConditionCounter(int condition, EvalType eval)
    {
        this.condition = condition;
        this.eval = eval;
    }

    protected override bool EvaluateCondition()
    {
        switch(eval)
        {
            case EvalType.LESS:
                return counter < condition;
            case EvalType.GREATER:
                return counter > condition;
            case EvalType.LESS_EQUAL:
                return counter <= condition;
            case EvalType.GREATER_EQUAL:
                return condition >= counter;
            case EvalType.EQUAL:
                return counter == condition;
            default:
                return false;
        }
    }

    public void Increment(int value = 1)
    {
        counter += value;
    }
    public void Decrement(int value = -1)
    {
        counter = counter - value < 0 ? 0 : counter - value;
    }
}
