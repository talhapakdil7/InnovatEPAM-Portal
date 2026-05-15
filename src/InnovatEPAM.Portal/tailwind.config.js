/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./Views/**/*.cshtml", "./wwwroot/**/*.{js,html}"],
  theme: {
    extend: {},
  },
  plugins: [require("@tailwindcss/forms")],
};
