// src/app/features/landing/landing.component.ts
import { Component, HostListener, inject, signal } from '@angular/core';
import { NavigationService } from '../../core/services/navigation.service';

interface ServiceItem {
  mark: string;
  title: string;
  body: string;
}

interface StepItem {
  index: string;
  title: string;
  body: string;
}

interface FaqItem {
  question: string;
  answer: string;
  open: boolean;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
})
export class LandingComponent {
  private readonly navigationService = inject(NavigationService);

  readonly year = new Date().getFullYear();

  /** True once the page has scrolled past the hero — toggles the compact header style. */
  readonly scrolled = signal(false);

  /** Mobile nav drawer open state. */
  readonly menuOpen = signal(false);

  readonly services: ServiceItem[] = [
    {
      mark: 'Records',
      title: 'Track your health record',
      body: 'Lab results, prescriptions, and visit notes live in one secure, organized record you can revisit anytime.',
    },
    {
      mark: 'Appointments',
      title: 'Book appointments',
      body: 'Find a time that works and book with your doctor in a few clicks — no calls, no back-and-forth.',
    },
    {
      mark: 'History',
      title: 'Track your history',
      body: 'See your full care timeline at a glance, from past visits to ongoing treatment plans.',
    },
    {
      mark: 'Doctors',
      title: 'Look up doctors',
      body: 'Search for doctors by specialty or availability and connect with the right care team faster.',
    },
  ];

  readonly steps: StepItem[] = [
    {
      index: '01',
      title: 'Create your account',
      body: 'Sign up as a patient, doctor, or admin in just a couple of minutes.',
    },
    {
      index: '02',
      title: 'Set up your profile',
      body: 'Add your details, link existing records, or set your availability.',
    },
    {
      index: '03',
      title: 'Start coordinating care',
      body: 'Book appointments, track your history, and stay in sync with your care team.',
    },
  ];

  readonly faqs: FaqItem[] = [

    {
      question: 'Can I book appointments with multiple doctors?',
      answer:
        'Yes. You can search for doctors, book appointments, and manage all of them from a single dashboard.',
      open: false,
    },
    {
      question: 'What does it cost to create an account?',
      answer:
        'Creating a HealthAxis account is free for patients. You can explore the platform before deciding what fits your needs.',
      open: false,
    },
  ];

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.scrolled.set(window.scrollY > 24);
  }

  toggleMenu(): void {
    this.menuOpen.set(!this.menuOpen());
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  toggleFaq(item: FaqItem): void {
    item.open = !item.open;
  }

  goToLogin(): void {
    this.navigationService.toLogin();
  }

  goToRegister(): void {
    this.navigationService.toRegister();
  }
}