using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public abstract class Player : Container {
    
    public Dbref Home = Dbref.NOT_FOUND;

    public override Dbref Link => Home;

    public override Dbref Owner => this.id;
}