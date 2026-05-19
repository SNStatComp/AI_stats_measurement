export function getResultTheme(name: string) {
  const value = (name ?? '').toLowerCase()

  if (value.includes('cbs')) {
    return {
      color: '#0580a1',
    }
  }

  if (value.includes('oecd')) {
    return {
      color: '#101d40',
    }
  }

  return {
    color: '#0f78c8',
  }
}