using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public abstract class Player : Container {
    
    public override Dbref Owner => this.id;
}