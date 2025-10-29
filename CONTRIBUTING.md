# Contributing to no-stick

Thank you for your interest in contributing to no-stick! This document provides guidelines for contributing to the project.

## 🎯 Project Vision

no-stick aims to eliminate the need for USB sticks when booting Linux distributions. We want to make it easy, safe, and reliable to boot ISOs directly from your hard drive.

## 🤝 How to Contribute

### Reporting Bugs

1. **Check existing issues** - Someone may have already reported it
2. **Create a detailed report** including:
   - Your Linux distribution and version
   - .NET version (`dotnet --version`)
   - Steps to reproduce
   - Expected vs actual behavior
   - Relevant logs/screenshots

### Suggesting Features

1. **Check roadmap** in README.md - it might already be planned
2. **Open an issue** with:
   - Clear use case description
   - Why this feature would be valuable
   - Proposed implementation (optional)

### Code Contributions

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/wallisoft/no-stick.git
cd no-stick

# Build the project
dotnet build

# Run tests (when available)
dotnet test
```

#### Coding Standards

- **C# Conventions**: Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Naming**: Use descriptive names for variables, methods, and classes
- **Comments**: Add comments for complex logic, but prefer self-documenting code
- **Error Handling**: Always handle exceptions gracefully
- **Async/Await**: Use async/await for I/O operations

#### Pull Request Process

1. **Fork the repository** and create a feature branch
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Write clean, documented code
   - Test thoroughly (especially bootloader operations!)
   - Update README.md if adding features

3. **Commit with clear messages**
   ```bash
   git commit -m "Add: ISO download functionality"
   git commit -m "Fix: GRUB detection on UEFI systems"
   ```

4. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **PR Review**
   - Respond to feedback promptly
   - Make requested changes
   - Keep PRs focused on a single feature/fix

## 🚨 Safety Guidelines

**no-stick modifies bootloaders, which can make systems unbootable if done incorrectly.**

When contributing bootloader-related code:
- ✅ Always test in a VM first
- ✅ Provide clear warnings to users
- ✅ Implement rollback mechanisms
- ✅ Validate all inputs rigorously
- ❌ Never assume user has backups (but remind them!)

## 📁 Project Structure

```
no-stick/
├── Models/           # Data models (ISOEntry, DistroEntry)
├── Managers/         # Business logic (GrubManager, etc.)
├── UI/
│   ├── DesktopMode/  # Full management UI
│   └── BootMode/     # Boot-time selector
├── Program.cs        # Entry point
└── *.csproj         # Project configuration
```

## 🧪 Testing

(Testing framework to be implemented)

- All bootloader operations should have integration tests
- UI components should have unit tests
- Test on multiple distributions when possible

## 📝 Documentation

- Update README.md for user-facing changes
- Add XML documentation comments for public APIs
- Update CHANGELOG.md for releases

## 💬 Communication

- **Issues**: For bugs, features, questions
- **Email**: wallisoft@gmail.com for sensitive topics
- **Website**: https://no-stick.uk

## 🎖️ Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md (to be created)
- Mentioned in release notes
- Credited in the project

## ⚖️ License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Questions?** Open an issue or email wallisoft@gmail.com

Thank you for helping make no-stick better! 🚀
