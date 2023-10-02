Func<DateTime, bool> canDrive= dob =>{
    return dob.AddYears(18)<=DateTime.Today;
};

DateTime dob = new DateTime(2000,12,25);
bool result=canDrive(dob);

Console.WriteLine("risultato:  "+result)
