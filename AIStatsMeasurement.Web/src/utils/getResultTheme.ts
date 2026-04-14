export function getResultTheme(name: string) {
  const lowerName = name.toLowerCase()

  if (lowerName.includes('cbs')) {
    return {
      color: '#0580a1',
    }
  }

  if (lowerName.includes('oecd')) {
    return {
      color: '#101d40',
    }
  }

  return {
    color: '#0f78c8',
  }
}