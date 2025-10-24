import { useState } from 'react'
import { X, Car, Palette, Settings, ChevronLeft, ChevronRight } from 'lucide-react'
import { fetchNui } from '../utils/fetchNui'
import '../ConcessAuto.css';

interface VehicleColor {
  id: number
  description: string
  hex: string
  rgb: string
}

interface Wheel {
  id: number
  name: string
}

interface WheelType {
  id: number
  name: string
  wheels: Wheel[]
}

interface ConcessAutoProps {
  data: {
    categories: Array<{
      name: string
      displayName: string
      vehicles: Array<{
        model: string
        name: string
        price: number
      }>
    }>
    colors: VehicleColor[]
    wheelTypes: WheelType[]
  }
  onClose: () => void
}

type Tab = 'vehicles' | 'colors' | 'wheels'

export default function ConcessAuto({ data, onClose }: ConcessAutoProps) {
  console.log('[CONCESS UI] Component mounted with data:', data)
  
  const [activeTab, setActiveTab] = useState<Tab>('vehicles')
  const [selectedCategory, setSelectedCategory] = useState(0)
  const [selectedVehicle, setSelectedVehicle] = useState<string | null>(null)
  const [primaryColor, setPrimaryColor] = useState(0)
  const [secondaryColor, setSecondaryColor] = useState(0)
  const [selectedWheelType, setSelectedWheelType] = useState(0)
  const [selectedWheelIndex, setSelectedWheelIndex] = useState(0)
  const [isMenuVisible, setIsMenuVisible] = useState(true)
  
  console.log('[CONCESS UI] Categories count:', data.categories?.length || 0)
  console.log('[CONCESS UI] Colors count:', data.colors?.length || 0)
  console.log('[CONCESS UI] Wheel types count:', data.wheelTypes?.length || 0)

  const handlePreviewVehicle = (model: string) => {
    console.log('[CONCESS] Prévisualisation:', model)
    setSelectedVehicle(model)
    fetchNui('nuiCallback', {
      callback: 'concess:previewVehicle',
      model: model
    })
  }

  const handleSetPrimaryColor = (colorId: number) => {
    setPrimaryColor(colorId)
    fetchNui('nuiCallback', {
      callback: 'concess:setPrimaryColor',
      colorId: colorId
    })
  }

  const handleSetSecondaryColor = (colorId: number) => {
    setSecondaryColor(colorId)
    fetchNui('nuiCallback', {
      callback: 'concess:setSecondaryColor',
      colorId: colorId
    })
  }

  const handleSetWheels = (wheelType: number, wheelIndex: number) => {
    setSelectedWheelType(wheelType)
    setSelectedWheelIndex(wheelIndex)
    fetchNui('nuiCallback', {
      callback: 'concess:setWheels',
      wheelType: wheelType,
      wheelIndex: wheelIndex
    })
  }

  const handleBuyVehicle = () => {
    if (!selectedVehicle) return
    
    fetchNui('nuiCallback', {
      callback: 'concess:buyVehicle'
    })
  }

  const handleClose = () => {
    fetchNui('nuiCallback', {
      callback: 'concess:closePreview'
    })
    onClose()
  }

  const toggleMenu = () => {
    setIsMenuVisible(!isMenuVisible)
  }

  return (
    <div className="concess-container">
      <div className={`concess-panel ${isMenuVisible ? 'visible' : 'hidden'}`}>
        <div className="concess-header">
          <h2>Concessionnaire Automobile</h2>
          <button onClick={handleClose} className="close-btn">
            <X size={24} />
          </button>
        </div>

        <div className="concess-tabs">
          <button
            className={`tab-btn ${activeTab === 'vehicles' ? 'active' : ''}`}
            onClick={() => setActiveTab('vehicles')}
          >
            <Car size={20} />
            <span>Véhicules</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'colors' ? 'active' : ''}`}
            onClick={() => setActiveTab('colors')}
          >
            <Palette size={20} />
            <span>Couleurs</span>
          </button>
          <button
            className={`tab-btn ${activeTab === 'wheels' ? 'active' : ''}`}
            onClick={() => setActiveTab('wheels')}
          >
            <Settings size={20} />
            <span>Roues</span>
          </button>
        </div>

        <div className="concess-content">
          {activeTab === 'vehicles' && (
            <div className="vehicles-section">
              <div className="categories-list">
                {data.categories.map((category, index) => (
                  <button
                    key={category.name}
                    className={`category-btn ${selectedCategory === index ? 'active' : ''}`}
                    onClick={() => setSelectedCategory(index)}
                  >
                    {category.displayName}
                  </button>
                ))}
              </div>
              <div className="vehicles-grid">
                {data.categories[selectedCategory]?.vehicles.map((vehicle) => (
                  <div
                    key={vehicle.model}
                    className={`vehicle-card ${selectedVehicle === vehicle.model ? 'selected' : ''}`}
                    onClick={() => handlePreviewVehicle(vehicle.model)}
                  >
                    <h3>{vehicle.name}</h3>
                    <p className="vehicle-model">{vehicle.model}</p>
                    <p className="vehicle-price">${vehicle.price.toLocaleString()}</p>
                  </div>
                ))}
              </div>
            </div>
          )}

          {activeTab === 'colors' && (
            <div className="colors-section">
              <div className="color-selector">
                <h3>Couleur Primaire</h3>
                <div className="colors-grid">
                  {data.colors.map((color) => (
                    <div
                      key={`primary-${color.id}`}
                      className={`color-item ${primaryColor === color.id ? 'selected' : ''}`}
                      onClick={() => handleSetPrimaryColor(color.id)}
                      title={color.description}
                    >
                      <div
                        className="color-preview"
                        style={{ backgroundColor: color.hex }}
                      />
                      <span className="color-name">{color.description}</span>
                    </div>
                  ))}
                </div>
              </div>
              <div className="color-selector">
                <h3>Couleur Secondaire</h3>
                <div className="colors-grid">
                  {data.colors.map((color) => (
                    <div
                      key={`secondary-${color.id}`}
                      className={`color-item ${secondaryColor === color.id ? 'selected' : ''}`}
                      onClick={() => handleSetSecondaryColor(color.id)}
                      title={color.description}
                    >
                      <div
                        className="color-preview"
                        style={{ backgroundColor: color.hex }}
                      />
                      <span className="color-name">{color.description}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {activeTab === 'wheels' && (
            <div className="wheels-section">
              <div className="wheel-types">
                {data.wheelTypes.map((wheelType) => (
                  <button
                    key={wheelType.id}
                    className={`wheel-type-btn ${selectedWheelType === wheelType.id ? 'active' : ''}`}
                    onClick={() => setSelectedWheelType(wheelType.id)}
                  >
                    {wheelType.name}
                  </button>
                ))}
              </div>
              <div className="wheels-grid">
                {data.wheelTypes
                  .find(wt => wt.id === selectedWheelType)
                  ?.wheels.map((wheel) => (
                    <div
                      key={wheel.id}
                      className={`wheel-card ${selectedWheelIndex === wheel.id ? 'selected' : ''}`}
                      onClick={() => handleSetWheels(selectedWheelType, wheel.id)}
                    >
                      <span>{wheel.name}</span>
                    </div>
                  ))}
              </div>
            </div>
          )}
        </div>

        {selectedVehicle && (
          <div className="concess-footer">
            <button className="buy-btn" onClick={handleBuyVehicle}>
              Acheter le véhicule
            </button>
          </div>
        )}
      </div>

      <button className="toggle-menu-btn" onClick={toggleMenu}>
        {isMenuVisible ? <ChevronLeft size={24} /> : <ChevronRight size={24} />}
      </button>
    </div>
  )
}