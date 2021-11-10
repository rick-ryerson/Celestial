﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GalacticSenate.Data.Interfaces
{
    public interface IRepositoryFactory
    {
        IGenderRepository GetGenderRepository();
    }
}