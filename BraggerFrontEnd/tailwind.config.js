module.exports = {
  darkMode: 'class', // Use class-based dark mode
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        primary: '#002D74', // Custom Blue for Light Mode
        secondary: {
          light: '#00bcd4', // Light Teal
          DEFAULT: '#009688', // Teal
          dark: '#00796b', // Darker Teal
        },
        // Dark Mode Colors
        'primary-dark': '#1E3A8A', // Darker Blue for Dark Mode
        'secondary-dark': {
          light: '#80DEEA', // Light Teal for Dark Mode
          DEFAULT: '#00796b', // Teal for Dark Mode
          dark: '#004d40', // Darker Teal for Dark Mode
        },
        // Other Colors
        lightblue: '#E0F7FA',
        skyblue: '#87CEEB',
        softblue: '#B3E5FC',
        powderblue: '#B0E0E6',
        steelblue: '#4682B4',
        deepskyblue: '#00BFFF',
        lightslateblue: '#8470FF',
        text: {
          light: '#333333', // Dark Gray for Light Mode
          DEFAULT: '#ffffff', // White Text for Light Mode
          dark: '#f5f5f5', // Light Gray for Dark Mode
        },
        success: '#4caf50', // Green for Success Messages
        error: '#f44336', // Red for Error Messages
      },
      animation: {
        'slide-up': 'slide-up 0.5s forwards',
        'fadeIn': 'fadeIn 3s ease-in-out infinite',
        'rotate': 'rotate 3s ease-in-out infinite',
      },
      keyframes: {
        'slide-up': {
          '0%': { transform: 'translateY(20px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'fadeIn': {
          '0%': {
            opacity: '0'
          },
          '50%': {
            opacity: '0.5'
          },
          '100%': {
            opacity: '1'
          }
        }
      },
    },
  },
  plugins: [],
};
