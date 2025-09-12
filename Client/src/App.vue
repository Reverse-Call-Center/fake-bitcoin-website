<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
    <Header />
    <main class="container mx-auto px-4 py-8">
      <RedemptionForm />
    </main>
    <Footer />
  </div>
</template>

<script setup lang="ts">
import Header from './components/Header.vue'
import RedemptionForm from './components/RedemptionForm.vue'
import Footer from './components/Footer.vue'
</script>
