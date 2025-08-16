
  function weekend_bonus(baseScore: number): number {
    const levelBonus =  1.05;
    return Math.floor(baseScore * levelBonus);
  }
  
  (globalThis as any).weekend_bonus = weekend_bonus;