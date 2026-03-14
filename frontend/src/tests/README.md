# Frontend Unit Tests

This directory contains comprehensive unit tests for the Quizzy frontend application built with Vue 3, Pinia, and Vuetify.

## Test Setup

- **Testing Framework**: Vitest
- **Component Testing**: @vue/test-utils
- **Environment**: jsdom

## Test Coverage

### Stores (Pinia)
- **auth.test.js**: Tests for authentication store including login, register, logout, and session management
- **quiz.test.js**: Tests for quiz store operations (fetch, create, update, delete)
- **leaderboard.test.js**: Tests for leaderboard data fetching and management

### Components
- **AuthModal.test.js**: Tests for authentication modal including form validation, login, and registration

### Views
- **HomePage.test.js**: Tests for home page component data and navigation logic
- **QuizList.test.js**: Tests for quiz list view including navigation and store integration

### Router
- **router.test.js**: Tests for route configuration and navigation guards (auth and admin requirements)

### API
- **api.test.js**: Tests for axios interceptors, authentication headers, and error handling

## Running Tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with UI
npm test:ui

# Generate coverage report
npm test:coverage
```

## Test Structure

Each test file follows this pattern:
- **Describe blocks**: Group related tests
- **beforeEach**: Set up fresh Pinia store and clear mocks
- **afterEach**: Cleanup mounted components
- **Mocks**: API calls and router navigation are mocked

## Test Results

✅ 74 tests passing
- 10 Auth Store tests
- 12 Quiz Store tests
- 8 Leaderboard Store tests
- 8 AuthModal Component tests
- 6 HomePage Component tests
- 6 QuizList Component tests
- 15 Router tests
- 9 API tests
