import { useState, useEffect } from 'react'
import Bank from './components/Bank'
import BankRobbery from './components/BankRobbery'
import PlayerMenu from './components/PlayerMenu'
import ConcessAuto from './components/ConcessAuto'
import { fetchNui } from './utils/fetchNui'
import { useNuiEvent } from './types/useNuiEvent'

interface BankData {
  bankName: string
  moneyInBank: number
  moneyInPocket: number
  playerName: string
}

interface RobberyData {
  bankName: string
  hasPC: boolean
  hasDrill: boolean
}

interface PlayerMenuData {
  playerInfo: {
    firstname: string
    lastname: string
    birth: string
    money: number
    cash: number
    bitcoin: number
    rank: string
    job: string
    jobRank: number
  }
  inventory: Array<{
    name: string
    quantity: number
    type: string
    icon: string
  }>
  weapons: Array<{
    name: string
    ammo: number
    icon: string
  }>
  clothes: Array<{
    name: string
    components: number
  }>
  bills: Array<{
    company: string
    author: string
    date: string
    amount: number
  }>
  vehicles: Array<{
    name: string
    model: string
    price: number
  }>
  isAdmin: boolean
  isDead?: boolean 
  players: Array<any>
}

interface ConcessAutoData {
  categories: Array<{
    name: string
    displayName: string
    vehicles: Array<{
      model: string
      name: string
      price: number
    }>
  }>
  colors: Array<{
    id: number
    description: string
    hex: string
    rgb: string
  }>
  wheelTypes: Array<{
    id: number
    name: string
    wheels: Array<{
      id: number
      name: string
    }>
  }>
}

function App() {
  const [isVisible, setIsVisible] = useState(false)
  const [currentNui, setCurrentNui] = useState('')
  const [bankData, setBankData] = useState<BankData>({
    bankName: '',
    moneyInBank: 0,
    moneyInPocket: 0,
    playerName: ''
  })
  const [robberyData, setRobberyData] = useState<RobberyData>({
    bankName: '',
    hasPC: false,
    hasDrill: false
  })
  const [playerMenuData, setPlayerMenuData] = useState<PlayerMenuData>({
    playerInfo: {
      firstname: '',
      lastname: '',
      birth: '',
      money: 0,
      cash: 0,
      bitcoin: 0,
      rank: '',
      job: '',
      jobRank: 0
    },
    inventory: [],
    weapons: [],
    clothes: [],
    bills: [],
    vehicles: [],
    isAdmin: false,
    isDead: false,
    players: []
  })
  const [concessAutoData, setConcessAutoData] = useState<ConcessAutoData>({
    categories: [],
    colors: [],
    wheelTypes: []
  })

  useNuiEvent('open', (data: any) => {
    console.log('[NUI] Open event received:', data)
    setCurrentNui(data.nui)
    setIsVisible(true)
    
    if (data.nui === 'bank' && data.data) {
      setBankData({
        bankName: data.data.bankName || 'Banque',
        moneyInBank: data.data.moneyInBank || 0,
        moneyInPocket: data.data.moneyInPocket || 0,
        playerName: data.data.playerName || 'Player'
      })
    } else if (data.nui === 'bankRobbery' && data.data) {
      setRobberyData({
        bankName: data.data.bankName || 'Banque',
        hasPC: data.data.hasPC || false,
        hasDrill: data.data.hasDrill || false
      })
    } else if (data.nui === 'playerMenu' && data.data) {
      setPlayerMenuData(data.data)
    } else if (data.nui === 'concessAuto' && data.data) {
      console.log('[NUI] ConcessAuto data type:', typeof data.data)
      console.log('[NUI] ConcessAuto data:', data.data)
      
      let parsedData = data.data
      if (typeof data.data === 'string') {
        try {
          parsedData = JSON.parse(data.data)
          console.log('[NUI] Parsed JSON data:', parsedData)
        } catch (e) {
          console.error('[NUI] Failed to parse JSON:', e)
          parsedData = { categories: [], colors: [], wheelTypes: [] }
        }
      }
      
      setConcessAutoData({
        categories: parsedData.categories || [],
        colors: parsedData.colors || [],
        wheelTypes: parsedData.wheelTypes || []
      })
      
      console.log('[NUI] ConcessAuto data set:', {
        categoriesCount: parsedData.categories?.length || 0,
        colorsCount: parsedData.colors?.length || 0,
        wheelTypesCount: parsedData.wheelTypes?.length || 0
      })
    }
  })

  useNuiEvent('updatePlayers', (data: any) => {
    if (data.players) {
      setPlayerMenuData(prev => ({
        ...prev,
        players: data.players
      }))
    }
  })

  useNuiEvent('close', () => {
    setIsVisible(false)
    setCurrentNui('')
  })

  useNuiEvent('notification', (data: any) => {
    console.log('[NUI] Notification:', data.data)
  })

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isVisible) {
        e.preventDefault()
        e.stopPropagation()
        handleClose()
      }
    }

    document.addEventListener('keydown', handleEscape, true)
    return () => document.removeEventListener('keydown', handleEscape, true)
  }, [isVisible, currentNui])

  const handleClose = () => {
    if (currentNui === 'concessAuto') {
      fetchNui('nuiCallback', {
        callback: 'concess:closePreview'
      })
    }
    fetchNui('closeNUI')
    setIsVisible(false)
    setCurrentNui('')
  }

  if (!isVisible) return null

  return (
    <div className="app-container">
      {currentNui === 'bank' && (
        <Bank 
          data={bankData}
          onClose={handleClose}
        />
      )}
      
      {currentNui === 'bankRobbery' && (
        <BankRobbery
          data={robberyData}
          onClose={handleClose}
        />
      )}

      {currentNui === 'playerMenu' && (
        <PlayerMenu
          data={playerMenuData}
          onClose={handleClose}
        />
      )}

      {currentNui === 'concessAuto' && (
        <ConcessAuto
          data={concessAutoData}
          onClose={handleClose}
        />
      )}
    </div>
  )
}

export default App