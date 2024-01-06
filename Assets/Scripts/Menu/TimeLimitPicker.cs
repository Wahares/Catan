public class TimeLimitPicker : ValuePicker
{
    public override string ToString()
    {
        if(value>0)
            return value/60 +":" + (value % 60 == 0?"00": value % 60);
        return "endless";
    }
}
