export function getResultTheme(name: string) {
  const lowerName = name.toLowerCase()

  if (lowerName.includes('cbs')) {
    return {
      color: '#2563eb',
      background: '#eff6ff'
    }
  }

  if (lowerName.includes('oecd')) {
    return {
      color: '#16a34a',
      background: '#f0fdf4'
    }
  }

  return {
    color: '#9333ea',
    background: '#faf5ff'
  }
}