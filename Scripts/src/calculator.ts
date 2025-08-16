
  function weekend_bonus(baseScore: number): number {
    //check if current date is weekend
    const currentDate = new Date();
    const isWeekend = currentDate.getDay() === 0 || currentDate.getDay() === 6;
    if(!isWeekend) {
      return baseScore;
    }
    //if current date is weekend, return baseScore * 1.05
    const levelBonus =  1.05;
    return Math.floor(baseScore * levelBonus);
  }
  
  (globalThis as any).weekend_bonus = weekend_bonus;